using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public interface IArchiveExtractor
{
    /// <summary>
    /// Extracts the files of <paramref name="settings"/> from all <paramref name="archiveFiles"/>, processing up to
    /// <see cref="ExtractionSettings.MaxDegreeOfParallelism"/> archives at the same time, and reports
    /// files that were not found in any archive. Destinations below the output path that go through a symbolic
    /// link or junction are skipped unless <see cref="ExtractionSettings.FollowSymbolicLinks"/> is set.
    /// </summary>
    Task<IReadOnlyCollection<FileInfoData>> ExtractFilesAsync(
        IReadOnlyCollection<string> archiveFiles,
        ExtractionSettings settings,
        CancellationToken cancellationToken);
}
