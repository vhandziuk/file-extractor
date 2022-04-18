using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Utils.Compression.Common;

public abstract class GenericArchiveExtractorBase : IArchiveExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly IGenericArchiveFileUtils _archiveFileUtils;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger _logger;

    protected GenericArchiveExtractorBase(
        IFileSystemUtils fileSystemUtils,
        IGenericArchiveFileUtils archiveFileUtils,
        ITaskRunner taskRunner,
        ILogger logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _archiveFileUtils = archiveFileUtils;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archiveFiles, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await _taskRunner.Run(
            () =>
            {
                var archives = Enumerable.Empty<IGenericArchive>();
                try
                {
                    archives = archiveFiles.Select(_archiveFileUtils.OpenRead);
                    var archiveEntries = GetEntries(archives);
                    return ExtractInternal(archiveEntries, outputPath, fileData);
                }
                finally
                {
                    foreach (var archive in archives)
                        archive.Dispose();
                }
            });

    private static IEnumerable<IGenericArchiveEntry> GetEntries(IEnumerable<IGenericArchive> archives) =>
        archives.SelectMany(archive => archive.Entries);

    private IEnumerable<FileInfoData> ExtractInternal(IEnumerable<IGenericArchiveEntry> archiveEntries, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        if (archiveEntries?.Any() != true)
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

        foreach (var pair in data)
        {
            var destinationPath = Path.Combine(extractedPath, pair.Key);
            if (!_fileSystemUtils.DirectoryExists(destinationPath))
                _fileSystemUtils.CreateDirectory(destinationPath);

            foreach (var archiveEntry in archiveEntries.AsParallel())
            {
                if (!TryGetMatchingEntry(archiveEntry, pair.Value, out var fileInfo))
                    continue;

                _logger.Information("Extracting {File} to {Path}", fileInfo.Name, destinationPath);
                archiveEntry.ExtractToFile(Path.Combine(destinationPath, fileInfo.Name), overwrite: true);
                extractedFiles.Add(fileInfo);
            }
        }

        return extractedFiles;
    }

    private static bool TryGetMatchingEntry(IGenericArchiveEntry entry, Dictionary<string, FileInfoData> data, out FileInfoData fileInfo) =>
        data.TryGetValue(entry.Name, out fileInfo)
        && (string.IsNullOrEmpty(data[entry.Name].Location)
            ? true
            : Path.GetDirectoryName(entry.FullName)?.EndsWith(data[entry.Name].Location, StringComparison.OrdinalIgnoreCase)) == true;

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}