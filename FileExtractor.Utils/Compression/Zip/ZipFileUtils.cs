namespace FileExtractor.Utils.Compression.Zip;

public sealed class ZipFileUtils : IZipFileUtils
{
    private readonly IZipFile _zipFile;

    public ZipFileUtils(IZipFile zipFile) =>
        _zipFile = zipFile;

    public IZipArchive OpenRead(string archiveFileName) =>
        _zipFile.OpenRead(archiveFileName);
}