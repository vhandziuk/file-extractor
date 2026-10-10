using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression.Rar;

internal sealed class RarArchiveWrapper(IRarArchive rarArchive, ISharpCompressEntryReader entryReader) : IGenericArchive
{
    public IEnumerable<IGenericArchiveEntry> Entries =>
        entryReader.ReadEntries(rarArchive, entry => new RarArchiveEntryWrapper(entry));

    public void Dispose() =>
        rarArchive.Dispose();
}
