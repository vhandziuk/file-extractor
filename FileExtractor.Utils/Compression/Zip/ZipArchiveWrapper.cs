using System.IO.Compression;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

internal sealed class ZipArchiveWrapper : IGenericArchive
{
    private readonly ZipArchive _zipArchive;

    public ZipArchiveWrapper(ZipArchive zipArchive) =>
        _zipArchive = zipArchive;

    // directory entries have an empty name
    public IEnumerable<IGenericArchiveEntry> Entries =>
        _zipArchive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name))
            .Select(entry => new ZipArchiveEntryWrapper(entry));

    public void Dispose() =>
        _zipArchive?.Dispose();
}
