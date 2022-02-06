using System.IO.Compression;

namespace FileExtractor.Utils.Compression;

public interface IZipFileUtils
{
    ZipArchive OpenRead(string archiveFileName);
}