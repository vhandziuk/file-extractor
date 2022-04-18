using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

internal sealed class SevenZipArchiveEntryWrapper : IGenericArchiveEntry
{
    private readonly SevenZipArchiveEntry _sevenZipArchiveEntry;

    public SevenZipArchiveEntryWrapper(SevenZipArchiveEntry rarArchiveEntry) =>
        _sevenZipArchiveEntry = rarArchiveEntry;

    public string Name => Path.GetFileName(_sevenZipArchiveEntry.Key);

    public string FullName => _sevenZipArchiveEntry.Key;

    public void ExtractToFile(string destinationFileName, bool overwrite) =>
        _sevenZipArchiveEntry.WriteToFile(
            destinationFileName,
            new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = overwrite
            });
}