using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

public interface ISevenZipFile
{
    IGenericArchive OpenRead(string archiveFileName);
}