using System.Collections.Concurrent;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public sealed class ExtractionContext(string outputPath, IReadOnlyList<FileInfoData> files) : IExtractionContext
{
    // Windows paths are case-insensitive
    private readonly ConcurrentDictionary<string, byte> _claimedFilePaths = new(StringComparer.OrdinalIgnoreCase);

    public string OutputPath => outputPath;

    public IReadOnlyList<FileInfoData> Files => files;

    public bool TryClaimFilePath(string path) => _claimedFilePaths.TryAdd(path, 0);
}
