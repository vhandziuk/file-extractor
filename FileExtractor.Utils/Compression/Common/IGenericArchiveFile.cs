namespace FileExtractor.Utils.Compression.Common;

public interface IGenericArchiveFile
{
    IGenericArchive OpenRead(string archiveFileName);
}