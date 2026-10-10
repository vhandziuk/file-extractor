using FileExtractor.Common.Logging;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

public sealed class ArchiveEntryExtractor(
    IFileSystemUtils fileSystemUtils,
    ILogger<ArchiveEntryExtractor> logger) : IArchiveEntryExtractor
{
    public IReadOnlyCollection<FileInfoData> ExtractEntries(
        string archiveFile,
        IEnumerable<IGenericArchiveEntry> entries,
        IExtractionContext context,
        CancellationToken cancellationToken)
    {
        var extractedFiles = new HashSet<FileInfoData>();
        var entryCount = 0;

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            entryCount++;
            extractedFiles.UnionWith(ExtractEntry(entry, context));
        }

        if (entryCount == 0)
            logger.Warning("Archive {Archive} contains no entries", archiveFile);

        return extractedFiles;
    }

    private FileInfoData[] ExtractEntry(IGenericArchiveEntry entry, IExtractionContext context)
    {
        var matchingFiles = context.Files
            .Where(fileInfo => ArchiveEntryMatcher.IsMatch(fileInfo, entry.Name, entry.FullName))
            .ToArray();

        // Several configuration rows (e.g. a name and a wildcard) can match the same entry and folder;
        // the entry is written to each distinct folder once.
        var destinationPaths = matchingFiles
            .Select(fileInfo => Path.Combine(context.OutputPath, fileInfo.Directory))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        string extractedFilePath = null;
        foreach (var destinationPath in destinationPaths)
            extractedFilePath = ExtractTo(entry, destinationPath, context, extractedFilePath);

        return matchingFiles;
    }

    private string ExtractTo(IGenericArchiveEntry entry, string destinationPath, IExtractionContext context, string extractedFilePath)
    {
        var destinationFilePath = Path.Combine(destinationPath, entry.Name);

        if (!context.TryClaimFilePath(destinationFilePath) || fileSystemUtils.FileExists(destinationFilePath))
        {
            logger.Warning("File {File} already exists in {Path}. Skipping extraction", entry.Name, destinationPath);
            return extractedFilePath;
        }

        fileSystemUtils.CreateDirectory(destinationPath);
        logger.Information("Extracting {File} to {Path}", entry.Name, destinationPath);
        WriteFile(entry, destinationFilePath, extractedFilePath);

        return extractedFilePath ?? destinationFilePath;
    }

    // A forward-only entry can be read only once, and decompressing it again would be slower anyway,
    // so every further destination receives a copy of the first extracted file.
    private void WriteFile(IGenericArchiveEntry entry, string destinationFilePath, string extractedFilePath)
    {
        try
        {
            if (extractedFilePath is null)
                entry.ExtractToFile(destinationFilePath, overwrite: true);
            else
                fileSystemUtils.Copy(extractedFilePath, destinationFilePath, true);
        }
        catch
        {
            // a partially written file would be skipped as "already exists" on the next run
            DeletePartialFile(destinationFilePath);
            throw;
        }
    }

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
