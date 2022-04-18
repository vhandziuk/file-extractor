using System.IO.Compression;
using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public sealed class ZipFileWrapper : IZipFile
{
    public IGenericArchive OpenRead(string archiveFileName) =>
        new ZipArchiveWrapper(ZipFile.OpenRead(archiveFileName));
}