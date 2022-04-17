namespace FileExtractor.Utils.Compression;

public interface IArchiveFile
{
    IArchive OpenRead(string archiveFileName);
}