using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

/// <summary>
/// State shared by all archives extracted in one run.
/// </summary>
public interface IExtractionContext
{
    string OutputPath { get; }

    IReadOnlyList<FileInfoData> Files { get; }

    /// <summary>
    /// Reserves a destination file for the caller. Returns <see langword="false"/> when another
    /// archive entry already claimed the same path, so every destination file has exactly one writer
    /// even when archives are extracted in parallel. Safe to call from multiple threads.
    /// </summary>
    bool TryClaimFilePath(string path);
}
