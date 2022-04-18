using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Rar;

public interface IRarFileUtils
{
    IGenericArchive OpenRead(string archiveFileName);
}