using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Rar;

public interface IRarFile
{
    IGenericArchive OpenRead(string archiveFileName);
}