using FileExtractor.Utils.Compression.Common;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace FileExtractor.Utils.Compression.Other;

internal sealed class OtherArchiveReaderEntryWrapper : IGenericArchiveEntry
{
    private readonly IReader _reader;
    private readonly string _key;

    public OtherArchiveReaderEntryWrapper(IReader reader)
    {
        _reader = reader;
        // single-file compressed archives (e.g. .gz) may not store an entry name
        _key = reader.Entry.Key ?? string.Empty;
    }

    public string Name => Path.GetFileName(_key);

    public string FullName => _key;

    public void ExtractToFile(string destinationFileName, bool overwrite) =>
        _reader.WriteEntryToFile(
            destinationFileName,
            new ExtractionOptions
            {
                ExtractFullPath = true,
                Overwrite = overwrite
            });
}
