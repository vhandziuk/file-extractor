using SharpCompress.Readers;

namespace FileExtractor.Utils.Compression.Other;

public sealed class OtherArchiveFileWrapper : IOtherArchiveFile
{
    public IOtherArchiveReader OpenReader(string archiveFileName)
    {
        var stream = File.OpenRead(archiveFileName);
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
