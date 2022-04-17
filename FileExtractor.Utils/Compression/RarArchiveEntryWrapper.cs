using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace FileExtractor.Utils.Compression;

internal sealed class RarArchiveEntryWrapper : IRarArchiveEntry
{
    private readonly RarArchiveEntry _rarArchiveEntry;

    public RarArchiveEntryWrapper(RarArchiveEntry rarArchiveEntry) =>
        _rarArchiveEntry = rarArchiveEntry;

    public string Name => Path.GetFileName(_rarArchiveEntry.Key);

    public string FullName => _rarArchiveEntry.Key;

    public void ExtractToFile(string destinationFileName, bool overwrite) =>
        _rarArchiveEntry.WriteToFile(
            destinationFileName,
            new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = overwrite
            });
}