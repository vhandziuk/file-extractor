namespace FileExtractor.Utils.Compression.SevenZip;

public interface ISevenZipFileUtils
{
    ISevenZipArchive OpenRead(string archiveFileName);
}