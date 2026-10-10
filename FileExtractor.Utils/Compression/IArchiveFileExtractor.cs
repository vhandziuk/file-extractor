using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

/// <summary>
/// Extracts the configured files from a single archive of one type. The work is synchronous and
/// CPU-bound; <see cref="IArchiveExtractor"/> decides how many archives run in parallel.
/// </summary>
public interface IArchiveFileExtractor
{
    /// <summary>
    /// Extracts the entries of <paramref name="archiveFile"/> that match <see cref="IExtractionContext.Files"/>
    /// and returns the configured files that were found in the archive.
    /// </summary>
    IReadOnlyCollection<FileInfoData> ExtractFiles(string archiveFile, IExtractionContext context, CancellationToken cancellationToken);
}
