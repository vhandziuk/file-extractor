namespace FileExtractor.Utils.Compression.Zip;

public interface IZipFile
{
    IZipArchive OpenRead(string archiveFileName);
}