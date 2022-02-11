namespace FileExtractor.Utils.Compression;

public interface IZipFileUtils
{
    IZipArchive OpenRead(string archiveFileName);
}