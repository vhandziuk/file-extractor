using System.IO.Compression;

namespace FileExtractor.Utils.Compression;

public sealed class ZipFileWrapper : IZipFile
{
    public IZipArchive OpenRead(string archiveFileName) => new ZipArchiveWrapper(ZipFile.OpenRead(archiveFileName));
}