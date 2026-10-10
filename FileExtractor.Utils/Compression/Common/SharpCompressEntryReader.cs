using FileExtractor.Utils.Compression.Other;
using SharpCompress.Archives;

namespace FileExtractor.Utils.Compression.Common;

public sealed class SharpCompressEntryReader : ISharpCompressEntryReader
{
    // Random access into a solid archive decompresses the solid block from its start for every
    // entry, so solid archives are read sequentially in a single pass instead.
    public IEnumerable<IGenericArchiveEntry> ReadEntries(IArchive archive, Func<IArchiveEntry, IGenericArchiveEntry> wrapEntry) =>
        archive.IsSolid
            ? ReadSequentially(archive)
            : archive.Entries.Where(entry => !entry.IsDirectory).Select(wrapEntry);

    private static IEnumerable<IGenericArchiveEntry> ReadSequentially(IArchive archive)
    {
        using var reader = archive.ExtractAllEntries();
        while (reader.MoveToNextEntry())
        {
            if (!reader.Entry.IsDirectory)
                yield return new OtherArchiveReaderEntryWrapper(reader);
        }
    }
}
