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
    /// Entries matching any of these files are not extracted, even when they match <see cref="Files"/>.
    /// </summary>
    IReadOnlyList<FileInfoData> ExcludedFiles { get; }

    /// <summary>
    /// Whether files may be written through symbolic links or junctions below <see cref="OutputPath"/>.
    /// </summary>
    bool FollowSymbolicLinks { get; }

    /// <summary>
    /// Whether an entry is written to its archive folder below the destination folder instead of directly into it.
    /// </summary>
    bool PreserveEntryPaths { get; }

    /// <summary>
    /// Whether each archive is extracted into a folder below <see cref="OutputPath"/> named after the archive.
    /// </summary>
    bool UseArchiveFolders { get; }

    /// <summary>
    /// Reserves a destination file for the caller. Returns <see langword="false"/> when another
    /// archive entry already claimed the same path, so every destination file has exactly one writer
    /// even when archives are extracted in parallel. Safe to call from multiple threads.
    /// </summary>
    bool TryClaimFilePath(string path);
}
