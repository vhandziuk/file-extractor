using System.IO.Compression;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

internal sealed class ZipArchiveEntryWrapper : IGenericArchiveEntry
{
    private readonly ZipArchiveEntry _zipArchiveEntry;

    public ZipArchiveEntryWrapper(ZipArchiveEntry zipArchiveEntry) =>
        _zipArchiveEntry = zipArchiveEntry;

    public string Name => _zipArchiveEntry.Name;

    public string FullName => _zipArchiveEntry.FullName;

    public void ExtractToFile(string destinationFileName, bool overwrite) =>
        _zipArchiveEntry.ExtractToFile(destinationFileName, overwrite);
}
