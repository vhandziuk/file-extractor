using SharpCompress.Archives.SevenZip;

namespace FileExtractor.Utils.Compression.SevenZip;

internal sealed class SevenZipArchiveWrapper : ISevenZipArchive
{
    private readonly SevenZipArchive _rarArchive;

    public SevenZipArchiveWrapper(SevenZipArchive rarArchive) =>
        _rarArchive = rarArchive;

    public IReadOnlyCollection<ISevenZipArchiveEntry> Entries =>
        _rarArchive.Entries.Select(entry => new SevenZipArchiveEntryWrapper(entry)).ToArray();

    public void Dispose() => _rarArchive?.Dispose();
}