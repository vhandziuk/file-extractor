using System.IO.Compression;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

internal sealed class ZipArchiveWrapper : IGenericArchive
{
    private readonly ZipArchive _zipArchive;

    public ZipArchiveWrapper(ZipArchive zipArchive) =>
        _zipArchive = zipArchive;

    public IReadOnlyCollection<IGenericArchiveEntry> Entries =>
        _zipArchive.Entries.Select(entry => new ZipArchiveEntryWrapper(entry)).ToArray();

    public void Dispose() =>
        _zipArchive?.Dispose();
}