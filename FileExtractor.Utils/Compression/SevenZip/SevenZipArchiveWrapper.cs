using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives;

namespace FileExtractor.Utils.Compression.SevenZip;

internal sealed class SevenZipArchiveWrapper(IArchive sevenZipArchive, ISharpCompressEntryReader entryReader) : IGenericArchive
{
    public IEnumerable<IGenericArchiveEntry> Entries =>
        entryReader.ReadEntries(sevenZipArchive, entry => new SevenZipArchiveEntryWrapper(entry));

    public void Dispose() =>
        sevenZipArchive.Dispose();
}
