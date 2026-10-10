using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileExtractor.Utils.Compression.Rar;

internal sealed class RarArchiveEntryWrapper : SharpCompressEntryWrapperBase
{
    private readonly IArchiveEntry _rarArchiveEntry;

    public RarArchiveEntryWrapper(IArchiveEntry rarArchiveEntry) : base(rarArchiveEntry) =>
        _rarArchiveEntry = rarArchiveEntry;

    // the default options validate the entry's CRC
    protected override void WriteTo(Stream stream) =>
        _rarArchiveEntry.WriteTo(stream, new ExtractionOptions());
}
