using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public interface IArchiveExtractor
{
    /// <summary>
    /// Extracts the configured files from all <paramref name="archiveFiles"/>, processing up to
    /// <paramref name="maxDegreeOfParallelism"/> archives at the same time, and reports configured
    /// files that were not found in any archive. Destinations below <paramref name="outputPath"/> that go
    /// through a symbolic link or junction are skipped unless <paramref name="followSymbolicLinks"/> is set.
    /// </summary>
    Task<IReadOnlyCollection<FileInfoData>> ExtractFilesAsync(
        IReadOnlyCollection<string> archiveFiles,
        string outputPath,
        IReadOnlyList<FileInfoData> fileData,
        int maxDegreeOfParallelism,
        bool followSymbolicLinks,
        CancellationToken cancellationToken);
}
