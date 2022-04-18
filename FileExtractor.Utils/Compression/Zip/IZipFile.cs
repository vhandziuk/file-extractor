using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public interface IZipFile
{
    IGenericArchive OpenRead(string archiveFileName);
}