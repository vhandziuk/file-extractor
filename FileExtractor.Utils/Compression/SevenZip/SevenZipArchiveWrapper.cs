using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives.SevenZip;

namespace FileExtractor.Utils.Compression.SevenZip;

internal sealed class SevenZipArchiveWrapper : IGenericArchive
{
    private readonly SevenZipArchive _sevenZipArchive;

    public SevenZipArchiveWrapper(SevenZipArchive sevenZipArchive) =>
        _sevenZipArchive = sevenZipArchive;

    public IReadOnlyCollection<IGenericArchiveEntry> Entries =>
        _sevenZipArchive.Entries.Select(entry => new SevenZipArchiveEntryWrapper(entry)).ToArray();

    public void Dispose() =>
        _sevenZipArchive?.Dispose();
}