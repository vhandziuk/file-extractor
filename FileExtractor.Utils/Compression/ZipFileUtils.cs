using System.IO.Compression;

namespace FileExtractor.Utils.Compression;

public sealed class ZipFileUtils : IZipFileUtils
{
    public ZipArchive OpenRead(string archiveFileName) => ZipFile.OpenRead(archiveFileName);
}