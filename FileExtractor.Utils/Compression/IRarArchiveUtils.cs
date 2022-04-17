namespace FileExtractor.Utils.Compression;

public interface IRarArchiveUtils
{
    IRarArchive OpenRead(string archiveFileName);
}