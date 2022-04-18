using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public sealed class ZipFileUtils : GenericArchiveFileUtilsBase, IZipFileUtils
{
    public ZipFileUtils(IZipFile zipFile)
        : base(zipFile)
    {
    }
}