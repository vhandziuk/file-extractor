namespace FileExtractor.Utils.Compression.Rar;

public interface IRarFileUtils
{
    IRarArchive OpenRead(string archiveFileName);
}