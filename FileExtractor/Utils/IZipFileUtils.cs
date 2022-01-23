using System.IO.Compression;

namespace FileExtractor.Utils;

internal interface IZipFileUtils
{
    ZipArchive OpenRead(string archiveFileName);
}