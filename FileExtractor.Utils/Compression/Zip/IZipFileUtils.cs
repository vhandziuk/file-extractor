namespace FileExtractor.Utils.Compression.Zip;

public interface IZipFileUtils
{
    IZipArchive OpenRead(string archiveFileName);
}