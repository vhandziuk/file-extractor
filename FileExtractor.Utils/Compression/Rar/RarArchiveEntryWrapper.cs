using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileExtractor.Utils.Compression.Rar;

internal sealed class RarArchiveEntryWrapper : IGenericArchiveEntry
{
    private readonly IArchiveEntry _rarArchiveEntry;

    public RarArchiveEntryWrapper(IArchiveEntry rarArchiveEntry) =>
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