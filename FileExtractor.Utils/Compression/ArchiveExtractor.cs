using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractor : IArchiveExtractor
{
    private readonly IArchiveExtractorFactory _archiveExtractorFactory;

    public ArchiveExtractor(IArchiveExtractorFactory archiveExtractorFactory) =>
        _archiveExtractorFactory = archiveExtractorFactory;

    public async Task ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData) =>
        await Task.WhenAll(archives
            .GroupBy(archive => GetArchiveType(Path.GetExtension(archive)))
            .ToDictionary(group => group.Key, group => group.ToArray())
            .Select(data => _archiveExtractorFactory.Create(data.Key).ExtractFiles(data.Value, outputPath, fileData)));

    private static ArchiveType GetArchiveType(string archiveExtension) =>
        archiveExtension.ToLowerInvariant() switch
        {
            ".zip" => ArchiveType.Zip,
            ".rar" => ArchiveType.Rar,
            _ => throw new Exception("Unsupported archive type")
        };
}