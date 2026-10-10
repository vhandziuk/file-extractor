using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

internal sealed class SevenZipArchiveEntryWrapper : SharpCompressEntryWrapperBase
{
    private readonly IArchiveEntry _sevenZipArchiveEntry;

    public SevenZipArchiveEntryWrapper(IArchiveEntry sevenZipArchiveEntry) : base(sevenZipArchiveEntry) =>
        _sevenZipArchiveEntry = sevenZipArchiveEntry;

    // the default options validate the entry's CRC
    protected override void WriteTo(Stream stream) =>
        _sevenZipArchiveEntry.WriteTo(stream, new ExtractionOptions());
}
