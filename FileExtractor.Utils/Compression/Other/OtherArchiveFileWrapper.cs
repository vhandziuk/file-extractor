using SharpCompress.Readers;

namespace FileExtractor.Utils.Compression.Other;

public sealed class OtherArchiveFileWrapper : IOtherArchiveFile
{
    private const int BufferSize = 81920;

    public IOtherArchiveReader OpenReader(string archiveFileName)
    {
        // the reader only moves forward, so a larger buffer and a sequential scan hint speed up reading
        var stream = new FileStream(archiveFileName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);
        try
        {
            return new OtherArchiveReaderWrapper(stream, ReaderFactory.OpenReader(stream));
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }
}
