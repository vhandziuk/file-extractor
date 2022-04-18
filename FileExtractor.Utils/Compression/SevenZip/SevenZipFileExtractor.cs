using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileExtractor : ISevenZipFileExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ISevenZipFileUtils _sevenZipFileUtils;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger<SevenZipFileExtractor> _logger;

    public SevenZipFileExtractor(
        IFileSystemUtils fileSystemUtils,
        ISevenZipFileUtils sevenZipFileUtils,
        ITaskRunner taskRunner,
        ILogger<SevenZipFileExtractor> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _sevenZipFileUtils = sevenZipFileUtils;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await _taskRunner.Run(
            () =>
            {
                var sevenZipArchives = Enumerable.Empty<ISevenZipArchive>();
                try
                {
                    sevenZipArchives = archives.Select(_sevenZipFileUtils.OpenRead);
                    var sevenZipEntries = GetEntries(sevenZipArchives);
                    return ExtractInternal(sevenZipEntries, outputPath, fileData);
                }
                finally
                {
                    foreach (var archive in sevenZipArchives)
                        archive.Dispose();
                }
            });

    private static IEnumerable<ISevenZipArchiveEntry> GetEntries(IEnumerable<ISevenZipArchive> sevenZipArchives) =>
        sevenZipArchives.SelectMany(archive => archive.Entries);

    private IEnumerable<FileInfoData> ExtractInternal(IEnumerable<ISevenZipArchiveEntry> sevenZipEntries, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        if (sevenZipEntries?.Any() != true)
        {
            _logger.Warning("The supplied archive(s) contain no entries");
            return Enumerable.Empty<FileInfoData>();
        }

        var data = fileData
            .GroupBy(entry => entry.Directory)
            .ToDictionary(group => group.Key, group => group.ToDictionary(entry => entry.Name));

        var extractedPath = GetExtractedPath(outputPath);
        if (!_fileSystemUtils.DirectoryExists(extractedPath))
            _fileSystemUtils.CreateDirectory(extractedPath);

        var extractedFiles = new ConcurrentBag<FileInfoData>();

        foreach (var pair in data.AsParallel())
        {
            var destinationPath = Path.Combine(extractedPath, pair.Key);
            if (!_fileSystemUtils.DirectoryExists(destinationPath))
                _fileSystemUtils.CreateDirectory(destinationPath);

            foreach (var sevenZipEntry in sevenZipEntries.AsParallel())
            {
                if (!TryGetMatchingEntry(sevenZipEntry, pair.Value, out var fileInfo))
                    continue;

                _logger.Information("Extracting {File} to {Path}", fileInfo.Name, destinationPath);
                sevenZipEntry.ExtractToFile(Path.Combine(destinationPath, fileInfo.Name), overwrite: true);
                extractedFiles.Add(fileInfo);
            }
        }

        return extractedFiles;
    }

    private static bool TryGetMatchingEntry(ISevenZipArchiveEntry entry, Dictionary<string, FileInfoData> data, out FileInfoData fileInfo) =>
        data.TryGetValue(entry.Name, out fileInfo)
        && (string.IsNullOrEmpty(data[entry.Name].Location)
            ? true
            : Path.GetDirectoryName(entry.FullName)?.EndsWith(data[entry.Name].Location, StringComparison.OrdinalIgnoreCase)) == true;

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}