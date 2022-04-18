using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

public interface ISevenZipFileUtils
{
    IGenericArchive OpenRead(string archiveFileName);
}