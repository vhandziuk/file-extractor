using System.IO.Compression;

namespace FileExtractor.Utils.Compression.Zip;

internal sealed class ZipArchiveWrapper : IZipArchive
{
    private readonly ZipArchive _zipArchive;

    public ZipArchiveWrapper(ZipArchive zipArchive) =>
        _zipArchive = zipArchive;

    public IReadOnlyCollection<IZipArchiveEntry> Entries =>
        _zipArchive.Entries.Select(entry => new ZipArchiveEntryWrapper(entry)).ToArray();

    public void Dispose() => _zipArchive?.Dispose();
}