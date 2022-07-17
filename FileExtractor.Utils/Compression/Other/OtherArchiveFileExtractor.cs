using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace FileExtractor.Utils.Compression.Other;

public sealed class OtherArchiveFileExtractor : IOtherArchiveFileExtractor
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger<OtherArchiveFileExtractor> _logger;

    public OtherArchiveFileExtractor(
        IFileSystemUtils fileSystemUtils,
        ITaskRunner taskRunner,
        ILogger<OtherArchiveFileExtractor> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await _taskRunner.Run(
            () =>
            {
                var data = fileData
                    .GroupBy(entry => entry.Directory)
                    .ToDictionary(group => group.Key, group => group.ToDictionary(entry => entry.Name));

                var extractedPath = GetExtractedPath(outputPath);
                if (!_fileSystemUtils.DirectoryExists(extractedPath))
                    _fileSystemUtils.CreateDirectory(extractedPath);

                var extractedFiles = new ConcurrentBag<FileInfoData>();

                foreach (var archive in archives.AsParallel())
                {
                    using (var stream = File.OpenRead(archive))
                    using (var reader = ReaderFactory.Open(stream))
                    {
                        while (reader.MoveToNextEntry())
                        {
                            if (reader.Entry.IsDirectory)
                                continue;

                            foreach (var pair in data)
                            {
                                if (!TryGetMatchingEntry(reader.Entry, pair.Value, out var fileInfo))
                                    continue;

                                var destinationPath = Path.Combine(extractedPath, pair.Key);
                                if (!_fileSystemUtils.DirectoryExists(destinationPath))
                                    _fileSystemUtils.CreateDirectory(destinationPath);

                                var extractedFilePath = Path.Combine(destinationPath, fileInfo.Name);
                                if (_fileSystemUtils.FileExists(extractedFilePath))
                                {
                                    _logger.Warning("File {File} already exists in {Path}. Skipping extraction", fileInfo.Name, destinationPath);
                                    extractedFiles.Add(fileInfo);
                                    continue;
                                }

                                _logger.Information("Extracting {File} to {Path}", fileInfo.Name, destinationPath);
                                reader.WriteEntryToFile(
                                    extractedFilePath,
                                    new ExtractionOptions
                                    {
                                        ExtractFullPath = true,
                                        Overwrite = true
                                    });
                                extractedFiles.Add(fileInfo);
                                break;
                            }
                        }
                    }
                }

                return extractedFiles;
            });

    private static bool TryGetMatchingEntry(IEntry entry, Dictionary<string, FileInfoData> data, out FileInfoData fileInfo)
    {
        var entryName = Path.GetFileName(entry.Key);
        var entryFullName = entry.Key;
        return data.TryGetValue(entryName, out fileInfo)
        && (string.IsNullOrEmpty(data[entryName].Location)
            ? true
            : Path.GetDirectoryName(entryFullName)?.EndsWith(data[entryName].Location, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static string GetExtractedPath(string outputPath) =>
        outputPath.EndsWith(value: "Extracted", StringComparison.OrdinalIgnoreCase)
            ? outputPath
            : Path.Combine(outputPath, "Extracted");
}