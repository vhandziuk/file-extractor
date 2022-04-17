namespace FileExtractor.Utils.Compression;

public interface IRarFile
{
    IRarArchive Open(string archiveFileName);
}