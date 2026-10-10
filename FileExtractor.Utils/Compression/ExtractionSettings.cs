using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

/// <summary>
/// What one extraction run writes and how: the files to extract into <see cref="OutputPath"/>
/// and the number of archives processed at the same time.
/// </summary>
public sealed record ExtractionSettings(string OutputPath, IReadOnlyList<FileInfoData> Files, int MaxDegreeOfParallelism)
{
    /// <summary>
    /// Whether files may be written through symbolic links or junctions below <see cref="OutputPath"/>.
    /// </summary>
    public bool FollowSymbolicLinks { get; init; }

    /// <summary>
    /// Whether the folders of an entry inside the archive are recreated below the destination folder.
    /// </summary>
    public bool PreserveEntryPaths { get; init; }

    /// <summary>
    /// Whether each archive is extracted into a folder below <see cref="OutputPath"/> named after the archive.
    /// </summary>
    public bool UseArchiveFolders { get; init; }
}
