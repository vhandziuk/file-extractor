namespace FileExtractor.Utils.Compression.SevenZip;

public interface ISevenZipFile
{
    ISevenZipArchive OpenRead(string archiveFileName);
}