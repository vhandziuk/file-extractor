namespace FileExtractor.Utils.Compression.Other;

public interface IOtherArchiveFile
{
    IOtherArchiveReader OpenReader(string archiveFileName);
}
