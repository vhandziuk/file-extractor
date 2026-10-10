using FileExtractor.Utils.Compression.Common;
using SharpCompress.Readers;

namespace FileExtractor.Utils.Compression.Other;

internal sealed class OtherArchiveReaderWrapper : IOtherArchiveReader
{
    private readonly Stream _stream;
    private readonly IReader _reader;

    public OtherArchiveReaderWrapper(Stream stream, IReader reader)
    {
        _stream = stream;
        _reader = reader;
    }

    public IGenericArchiveEntry Entry => new OtherArchiveReaderEntryWrapper(_reader);

    public bool MoveToNextEntry()
    {
        while (_reader.MoveToNextEntry())
        {
            if (!_reader.Entry.IsDirectory)
                return true;
        }

        return false;
    }

    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }
}
