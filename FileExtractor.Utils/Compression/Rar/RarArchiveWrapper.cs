using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives.Rar;

namespace FileExtractor.Utils.Compression.Rar;

internal sealed class RarArchiveWrapper : IGenericArchive
{
    private readonly IRarArchive _rarArchive;

    public RarArchiveWrapper(IRarArchive rarArchive) =>
        _rarArchive = rarArchive;

    public IEnumerable<IGenericArchiveEntry> Entries =>
        SharpCompressArchiveEntries.Enumerate(_rarArchive, entry => new RarArchiveEntryWrapper(entry));

    public void Dispose() =>
        _rarArchive?.Dispose();
}
