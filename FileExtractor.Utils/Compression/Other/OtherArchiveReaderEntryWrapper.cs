using FileExtractor.Utils.Compression.Common;
using SharpCompress.Readers;

namespace FileExtractor.Utils.Compression.Other;

internal sealed class OtherArchiveReaderEntryWrapper : SharpCompressEntryWrapperBase
{
    private readonly IReader _reader;

    public OtherArchiveReaderEntryWrapper(IReader reader) : base(reader.Entry) =>
        _reader = reader;

    protected override void WriteTo(Stream stream) =>
        _reader.WriteEntryTo(stream);
}
