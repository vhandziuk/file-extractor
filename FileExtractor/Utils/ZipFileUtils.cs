using System.IO.Compression;

namespace FileExtractor.Utils;

internal sealed class ZipFileUtils : IZipFileUtils
{
    public ZipArchive OpenRead(string archiveFileName) => ZipFile.OpenRead(archiveFileName);
}