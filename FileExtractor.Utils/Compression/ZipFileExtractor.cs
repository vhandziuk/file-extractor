using System.Collections.Concurrent;
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

        var data = fileData
            .GroupBy(entry => entry.Directory)
            .ToDictionary(group => group.Key, group => group.ToDictionary(entry => entry.Name));

        var extractedPath = GetExtractedPath(outputPath);
        if (!_fileSystemUtils.DirectoryExists(extractedPath))
            _fileSystemUtils.CreateDirectory(extractedPath);

        _logger.Information("Processing files");

        var extractedFiles = new ConcurrentBag<FileInfoData>();
        foreach (var pair in data.AsParallel())
        {
            var destinationPath = Path.Combine(extractedPath, pair.Key);
            if (!_fileSystemUtils.DirectoryExists(destinationPath))
                _fileSystemUtils.CreateDirectory(destinationPath);

            foreach (var zipEntry in zipEntries.AsParallel())
            {
                if (!TryGetMatchingEntry(zipEntry, pair.Value, out var fileInfo))
                    continue;

                _logger.Information("Extracting {File} to {Path}", fileInfo.Name, destinationPath);
                zipEntry.ExtractToFile(Path.Combine(destinationPath, fileInfo.Name), overwrite: true);
                extractedFiles.Add(fileInfo);
            }
        }

        var sortedFileData = fileData
            .OrderBy(fileInfo => fileInfo.Directory)
            .ThenBy(fileInfo => fileInfo.Name)
            .ToHashSet();

        sortedFileData.SymmetricExceptWith(extractedFiles);
        if (!sortedFileData.Any())
        {
            _logger.Information("Processing completed. All files have been successfully extracted");
            return;
        }
        _logger.Warning("Processing completed. Missing files detected");
        foreach (var file in sortedFileData)
            _logger.Warning("File {File} was not found in the supplied archive(s)", Path.Combine(file.Location, file.Name));
    }

    private static bool TryGetMatchingEntry(IZipArchiveEntry entry, Dictionary<string, FileInfoData> data, out FileInfoData fileInfo) =>
        data.TryGetValue(entry.Name, out fileInfo)
        && (string.IsNullOrEmpty(data[entry.Name].Location)
            ? true
            : Path.GetDirectoryName(entry.FullName)?.EndsWith(data[entry.Name].Location, StringComparison.OrdinalIgnoreCase)) == true;

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}