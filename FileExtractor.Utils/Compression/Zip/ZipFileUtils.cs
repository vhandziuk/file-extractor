using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public sealed class ZipFileUtils : IZipFileUtils
{
    private readonly IZipFile _zipFile;

    public ZipFileUtils(IZipFile zipFile) =>
        _zipFile = zipFile;

    public IGenericArchive OpenRead(string archiveFileName) =>
        _zipFile.OpenRead(archiveFileName);
}