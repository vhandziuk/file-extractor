namespace FileExtractor.Utils.Compression;

public interface IZipFile
{
    IZipArchive OpenRead(string archiveFileName);
}