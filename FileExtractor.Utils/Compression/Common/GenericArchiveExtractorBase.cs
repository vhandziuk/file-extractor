using FileExtractor.Data;

namespace FileExtractor.Utils.Compression.Common;

public abstract class GenericArchiveExtractorBase(
    IGenericArchiveFileUtils archiveFileUtils,
    IArchiveEntryExtractor archiveEntryExtractor) : IArchiveFileExtractor
{
    public IReadOnlyCollection<FileInfoData> ExtractFiles(string archiveFile, IExtractionContext context, CancellationToken cancellationToken)
    {
        using var archive = archiveFileUtils.OpenRead(archiveFile);

        return archiveEntryExtractor.ExtractEntries(archiveFile, archive.Entries, context, cancellationToken);
    }

    public IReadOnlyList<string> ListEntries(string archiveFile)
    {
        using var archive = archiveFileUtils.OpenRead(archiveFile);

        return [.. archive.Entries.Select(entry => entry.FullName)];
    }
}
