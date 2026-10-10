using FileExtractor.Data;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Other;

public sealed class OtherArchiveFileExtractor(
    IOtherArchiveFile otherArchiveFile,
    IArchiveEntryExtractor archiveEntryExtractor) : IOtherArchiveFileExtractor
{
    public IReadOnlyCollection<FileInfoData> ExtractFiles(string archiveFile, IExtractionContext context, CancellationToken cancellationToken)
    {
        using var reader = otherArchiveFile.OpenReader(archiveFile);

        return archiveEntryExtractor.ExtractEntries(archiveFile, ReadEntries(reader), context, cancellationToken);
    }

    private static IEnumerable<IGenericArchiveEntry> ReadEntries(IOtherArchiveReader reader)
    {
        while (reader.MoveToNextEntry())
            yield return reader.Entry;
    }
}
