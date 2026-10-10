using System.Collections.Concurrent;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public sealed class ExtractionContext(ExtractionSettings settings) : IExtractionContext
{
    // Windows paths are case-insensitive
    private readonly ConcurrentDictionary<string, byte> _claimedFilePaths = new(StringComparer.OrdinalIgnoreCase);

    public string OutputPath => settings.OutputPath;

    public IReadOnlyList<FileInfoData> Files => settings.Files;

    public bool FollowSymbolicLinks => settings.FollowSymbolicLinks;

    public bool PreserveEntryPaths => settings.PreserveEntryPaths;

    public bool TryClaimFilePath(string path) => _claimedFilePaths.TryAdd(path, 0);
}
