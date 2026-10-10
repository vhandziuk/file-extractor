using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

public interface IArchiveEntryExtractor
{
    /// <summary>
    /// Writes every entry that matches <see cref="IExtractionContext.Files"/> to its destination folder(s)
    /// and returns the configured files that were found. Entries are processed one at a time in a single
    /// pass, so forward-only entries (valid only during their own iteration step) are supported.
    /// </summary>
    IReadOnlyCollection<FileInfoData> ExtractEntries(
        string archiveFile,
        IEnumerable<IGenericArchiveEntry> entries,
        IExtractionContext context,
        CancellationToken cancellationToken);
}
