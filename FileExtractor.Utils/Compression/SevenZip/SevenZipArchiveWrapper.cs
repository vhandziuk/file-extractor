using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives;

namespace FileExtractor.Utils.Compression.SevenZip;

internal sealed class SevenZipArchiveWrapper : IGenericArchive
{
    private readonly IArchive _sevenZipArchive;

    public SevenZipArchiveWrapper(IArchive sevenZipArchive) =>
        _sevenZipArchive = sevenZipArchive;

    public IEnumerable<IGenericArchiveEntry> Entries =>
        SharpCompressArchiveEntries.Enumerate(_sevenZipArchive, entry => new SevenZipArchiveEntryWrapper(entry));

    public void Dispose() =>
        _sevenZipArchive?.Dispose();
}
