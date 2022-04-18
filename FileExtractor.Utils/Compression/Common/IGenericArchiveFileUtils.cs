namespace FileExtractor.Utils.Compression.Common;

public interface IGenericArchiveFileUtils
{
    IGenericArchive OpenRead(string archiveFileName);
}