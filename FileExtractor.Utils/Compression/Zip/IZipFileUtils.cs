using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public interface IZipFileUtils
{
    IGenericArchive OpenRead(string archiveFileName);
}