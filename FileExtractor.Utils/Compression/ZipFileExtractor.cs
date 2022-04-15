using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Utils.Compression;

public sealed class ZipFileExtractor : IZipFileExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly IZipFileUtils _zipFileUtils;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger<ZipFileExtractor> _logger;

    public ZipFileExtractor(
        IFileSystemUtils fileSystemUtils,
        IZipFileUtils zipFileUtils,
        ITaskRunner taskRunner,
        ILogger<ZipFileExtractor> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _zipFileUtils = zipFileUtils;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        await _taskRunner.Run(
            () =>
            {
                var zipArchives = Enumerable.Empty<IZipArchive>();
                try
                {
                    zipArchives = archives.Select(_zipFileUtils.OpenRead);
                    var zipEntries = GetEntries(zipArchives);
                    ExtractInternal(zipEntries, outputPath, fileData);
                }
                finally
                {
                    foreach (var archive in zipArchives)
                        archive.Dispose();
                }
            });
    }

    private static IEnumerable<IZipArchiveEntry> GetEntries(IEnumerable<IZipArchive> zipArchives) =>
        zipArchives.SelectMany(archive => archive.Entries);

    private void ExtractInternal(IEnumerable<IZipArchiveEntry> zipEntries, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        if (zipEntries?.Any() != true)
        {
            _logger.Warning("The supplied archive(s) contain no entries");
            return;
        }

        var data = fileData.ToDictionary(entry => entry.Name);

        var extractedPath = GetExtractedPath(outputPath);
        if (!_fileSystemUtils.DirectoryExists(extractedPath))
            _fileSystemUtils.CreateDirectory(extractedPath);

        _logger.Information("Processing files");

        var extractedFileNames = new SortedSet<string>();
        foreach (var zipEntry in zipEntries
            .Where(entry => ContainsEntry(entry, data))
            .AsParallel())
        {
            _logger.Information("Extracting {File} to {Path}", zipEntry.Name, extractedPath);
            zipEntry.ExtractToFile(Path.Combine(extractedPath, zipEntry.Name), overwrite: true);
            extractedFileNames.Add(zipEntry.Name);
        }

        extractedFileNames.SymmetricExceptWith(data.Keys);
        if (!extractedFileNames.Any())
        {
            _logger.Information("Processing completed. All files have been successfully extracted");
            return;
        }
        _logger.Warning("Processing completed. Missing files detected");
        foreach (var fileName in extractedFileNames)
            _logger.Warning("File {FileName} was not found in the supplied archive(s)", fileName);
    }

    private static bool ContainsEntry(IZipArchiveEntry entry, Dictionary<string, FileInfoData> data) =>
        data.ContainsKey(entry.Name)
        && (string.IsNullOrEmpty(data[entry.Name].DirectoryName)
            ? true
            : Path.GetDirectoryName(entry.FullName)?.EndsWith(data[entry.Name].DirectoryName, StringComparison.OrdinalIgnoreCase)) == true;

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}