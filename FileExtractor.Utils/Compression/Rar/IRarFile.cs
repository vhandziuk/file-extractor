namespace FileExtractor.Utils.Compression.Rar;

public interface IRarFile
{
    IRarArchive OpenRead(string archiveFileName);
}