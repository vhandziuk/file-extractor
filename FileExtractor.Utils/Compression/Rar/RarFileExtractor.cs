using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileExtractor : IRarFileExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly IRarFileUtils _rarFileUtils;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger<RarFileExtractor> _logger;

    public RarFileExtractor(
        IFileSystemUtils fileSystemUtils,
        IRarFileUtils rarFileUtils,
        ITaskRunner taskRunner,
        ILogger<RarFileExtractor> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _rarFileUtils = rarFileUtils;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await _taskRunner.Run(
            () =>
            {
                var rarArchives = Enumerable.Empty<IRarArchive>();
                try
                {
                    rarArchives = archives.Select(_rarFileUtils.OpenRead);
                    var rarEntries = GetEntries(rarArchives);
                    return ExtractInternal(rarEntries, outputPath, fileData);
                }
                finally
                {
                    foreach (var archive in rarArchives)
                        archive.Dispose();
                }
            });

    private static IEnumerable<IRarArchiveEntry> GetEntries(IEnumerable<IRarArchive> rarArchives) =>
        rarArchives.SelectMany(archive => archive.Entries);

    private IEnumerable<FileInfoData> ExtractInternal(IEnumerable<IRarArchiveEntry> rarEntries, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        if (rarEntries?.Any() != true)
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

            foreach (var rarEntry in rarEntries.AsParallel())
            {
                if (!TryGetMatchingEntry(rarEntry, pair.Value, out var fileInfo))
                    continue;

                _logger.Information("Extracting {File} to {Path}", fileInfo.Name, destinationPath);
                rarEntry.ExtractToFile(Path.Combine(destinationPath, fileInfo.Name), overwrite: true);
                extractedFiles.Add(fileInfo);
            }
        }

        return extractedFiles;
    }

    private static bool TryGetMatchingEntry(IRarArchiveEntry entry, Dictionary<string, FileInfoData> data, out FileInfoData fileInfo) =>
        data.TryGetValue(entry.Name, out fileInfo)
        && (string.IsNullOrEmpty(data[entry.Name].Location)
            ? true
            : Path.GetDirectoryName(entry.FullName)?.EndsWith(data[entry.Name].Location, StringComparison.OrdinalIgnoreCase)) == true;

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}