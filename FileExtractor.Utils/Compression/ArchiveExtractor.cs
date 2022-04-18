using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractor : IArchiveExtractor
{
    private readonly IArchiveExtractorFactory _archiveExtractorFactory;
    private readonly ITaskRunner _taskRunner;
    private readonly ILogger<ArchiveExtractor> _logger;

    public ArchiveExtractor(
        IArchiveExtractorFactory archiveExtractorFactory,
        ITaskRunner taskRunner,
        ILogger<ArchiveExtractor> logger)
    {
        _archiveExtractorFactory = archiveExtractorFactory;
        _taskRunner = taskRunner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData)
    {
        _logger.Information("Processing files");

        var result = await Task.WhenAll(archives
            .GroupBy(archive => GetArchiveType(Path.GetExtension(archive)))
            .ToDictionary(group => group.Key, group => group.ToArray())
            .Select(data => _taskRunner.Run(() => _archiveExtractorFactory.Create(data.Key).ExtractFiles(data.Value, outputPath, fileData))));

        var sortedFileData = fileData
            .OrderBy(fileInfo => fileInfo.Directory)
            .ThenBy(fileInfo => fileInfo.Name)
            .ToHashSet();

        var extractedFiles = result
            .SelectMany(extracted => extracted)
            .ToArray();

        sortedFileData.SymmetricExceptWith(extractedFiles);
        if (!sortedFileData.Any())
        {
            _logger.Information("Processing completed. All files have been successfully extracted");
            return extractedFiles;
        }
        _logger.Warning("Processing completed. Missing files detected");
        foreach (var file in sortedFileData)
            _logger.Warning("File {File} was not found in the supplied archive(s)", Path.Combine(file.Location, file.Name));

        return extractedFiles;
    }

    private static ArchiveType GetArchiveType(string archiveExtension) =>
        archiveExtension.ToLowerInvariant() switch
        {
            ".zip" => ArchiveType.Zip,
            ".rar" => ArchiveType.Rar,
            ".7z" => ArchiveType.SevenZip,
            ".tar" => ArchiveType.Other,
            ".bz2" => ArchiveType.Other,
            ".gz" => ArchiveType.Other,
            ".lz" => ArchiveType.Other,
            ".xz" => ArchiveType.Other,
            _ => throw new Exception("Unsupported archive type")
        };
}