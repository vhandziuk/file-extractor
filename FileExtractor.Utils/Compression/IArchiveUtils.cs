namespace FileExtractor.Utils.Compression;

public interface IArchiveUtils
{
    IArchive OpenRead(string archiveFileName);
}