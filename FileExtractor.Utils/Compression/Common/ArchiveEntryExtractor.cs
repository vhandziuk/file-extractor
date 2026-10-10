using FileExtractor.Common.Logging;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

public sealed class ArchiveEntryExtractor(
    IArchiveEntryMatcher archiveEntryMatcher,
    IPathValidator pathValidator,
    IReparsePointGuard reparsePointGuard,
    IFileSystemUtils fileSystemUtils,
    ILogger<ArchiveEntryExtractor> logger) : IArchiveEntryExtractor
{
    // ERROR_FILE_EXISTS and ERROR_ALREADY_EXISTS as HRESULTs
    private const int FileExistsHResult = unchecked((int)0x80070050);
    private const int AlreadyExistsHResult = unchecked((int)0x800700B7);

    private static readonly char[] _directorySeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    public IReadOnlyCollection<FileInfoData> ExtractEntries(
        string archiveFile,
        IEnumerable<IGenericArchiveEntry> entries,
        IExtractionContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetArchiveOutputPath(archiveFile, context, out var archiveOutputPath))
            return [];

        var extractedFiles = new HashSet<FileInfoData>();
        var entryCount = 0;

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            entryCount++;
            extractedFiles.UnionWith(ExtractEntry(entry, archiveOutputPath, context));
        }

        if (entryCount == 0)
            logger.Warning("Archive {Archive} contains no entries", archiveFile);

        return extractedFiles;
    }

    // With archive folders, each archive is extracted into a folder named after it, so entries with the same
    // name in different archives do not collide. The folder stays below the output path, where the
    // containment and link checks of every destination start.
    private bool TryGetArchiveOutputPath(string archiveFile, IExtractionContext context, out string archiveOutputPath)
    {
        archiveOutputPath = context.OutputPath;
        if (!context.UseArchiveFolders)
            return true;

        var folderName = Path.GetFileNameWithoutExtension(archiveFile);
        if (!pathValidator.IsValidFileName(folderName))
        {
            logger.Warning("Archive {Archive} cannot be extracted to a folder named {Folder}. Skipping it", archiveFile, folderName);
            return false;
        }

        archiveOutputPath = Path.Combine(context.OutputPath, folderName);
        return true;
    }

    private FileInfoData[] ExtractEntry(IGenericArchiveEntry entry, string archiveOutputPath, IExtractionContext context)
    {
        var matchingFiles = context.Files
            .Where(fileInfo => archiveEntryMatcher.IsMatch(fileInfo, entry.Name, entry.FullName))
            .ToArray();
        if (matchingFiles.Length == 0 || IsExcluded(entry, context))
            return [];

        // The archive is untrusted: a name such as "..\file.dll", stored by a non-Windows zip tool,
        // would otherwise be written outside the destination folder.
        if (!pathValidator.IsValidFileName(entry.Name))
        {
            logger.Warning("Archive entry {Entry} has an unsafe name. Skipping it", entry.FullName);
            return [];
        }

        var entryDirectory = GetEntryDirectory(entry, context);
        if (!pathValidator.IsValidRelativePath(entryDirectory))
        {
            logger.Warning("Archive entry {Entry} has an unsafe path. Skipping it", entry.FullName);
            return [];
        }

        // Several configuration rows (e.g. a name and a wildcard) can match the same entry and folder;
        // the entry is written to each distinct folder once.
        var destinations = matchingFiles
            .GroupBy(fileInfo => Path.Combine(archiveOutputPath, fileInfo.Directory, entryDirectory), StringComparer.OrdinalIgnoreCase);

        var extractedFiles = new List<FileInfoData>();
        string extractedFilePath = null;
        foreach (var destination in destinations)
        {
            var destinationFilePath = Path.Combine(destination.Key, entry.Name);

            // rows of an unsafe destination stay unextracted, so they are reported as missing
            if (!IsSafeDestination(destinationFilePath, context))
                continue;

            extractedFiles.AddRange(destination);
            extractedFilePath = ExtractTo(entry, destination.Key, destinationFilePath, context, extractedFilePath);
        }

        return [.. extractedFiles];
    }

    // An excluded entry is skipped silently and its matching rows stay unextracted, like an entry that does not match.
    private bool IsExcluded(IGenericArchiveEntry entry, IExtractionContext context) =>
        context.ExcludedFiles.Any(fileInfo => archiveEntryMatcher.IsMatch(fileInfo, entry.Name, entry.FullName));

    // The folders inside the archive, without the "." and empty segments that tar archives often store
    // ("./docs/file.txt"), so equal destinations get equal paths. They come from the untrusted archive
    // and are validated by the caller.
    private static string GetEntryDirectory(IGenericArchiveEntry entry, IExtractionContext context)
    {
        if (!context.PreserveEntryPaths)
            return string.Empty;

        var segments = entry.FullName.Split(_directorySeparators);

        return string.Join(Path.DirectorySeparatorChar, segments[..^1].Where(segment => segment is not ("" or ".")));
    }

    private string ExtractTo(
        IGenericArchiveEntry entry,
        string destinationPath,
        string destinationFilePath,
        IExtractionContext context,
        string extractedFilePath)
    {
        if (!context.TryClaimFilePath(destinationFilePath) || fileSystemUtils.FileExists(destinationFilePath))
        {
            LogFileExists(entry, destinationPath);
            return extractedFilePath;
        }

        fileSystemUtils.CreateDirectory(destinationPath);
        logger.Information("Extracting {File} to {Path}", entry.Name, destinationPath);
        if (!TryWriteFile(entry, destinationPath, destinationFilePath, extractedFilePath))
            return extractedFilePath;

        return extractedFilePath ?? destinationFilePath;
    }

    private bool IsSafeDestination(string destinationFilePath, IExtractionContext context)
    {
        var outputPath = context.OutputPath;
        if (!pathValidator.IsContained(outputPath, destinationFilePath))
        {
            logger.Warning("Destination {Path} is outside the output path {OutputPath}. Skipping extraction", destinationFilePath, outputPath);
            return false;
        }

        // Creating the folders and the file would follow a junction or symbolic link planted below the output path,
        // unless the user explicitly allowed it. The containment check above still applies to the path as written.
        if (!context.FollowSymbolicLinks && reparsePointGuard.ContainsReparsePoint(outputPath, destinationFilePath))
        {
            logger.Warning("Destination {Path} contains a symbolic link or junction. Skipping extraction", destinationFilePath);
            return false;
        }

        return true;
    }

    // A forward-only entry can be read only once, and decompressing it again would be slower anyway,
    // so every further destination receives a copy of the first extracted file.
    // Existing files are never overwritten, so a file or link that appears after the checks above
    // is neither followed nor replaced.
    private bool TryWriteFile(IGenericArchiveEntry entry, string destinationPath, string destinationFilePath, string extractedFilePath)
    {
        try
        {
            if (extractedFilePath is null)
                entry.ExtractToFile(destinationFilePath, overwrite: false);
            else
                fileSystemUtils.Copy(extractedFilePath, destinationFilePath, false);

            return true;
        }
        catch (IOException ex) when (ex.HResult is FileExistsHResult or AlreadyExistsHResult)
        {
            // not written by this run, so it must not be deleted as a partial file
            LogFileExists(entry, destinationPath);
            return false;
        }
        catch
        {
            // a partially written file would be skipped as "already exists" on the next run
            DeletePartialFile(destinationFilePath);
            throw;
        }
    }

    private void LogFileExists(IGenericArchiveEntry entry, string destinationPath) =>
        logger.Warning("File {File} already exists in {Path}. Skipping extraction", entry.Name, destinationPath);

    private void DeletePartialFile(string filePath)
    {
        try
        {
            fileSystemUtils.DeleteFile(filePath);
        }
        catch (Exception ex)
        {
            // keep the original extraction error; the cleanup failure is only logged
            logger.Error(ex, "Failed to delete partially extracted file {File}", filePath);
        }
    }
}
