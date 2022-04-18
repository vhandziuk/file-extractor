using FileExtractor.Utils.Compression.Common;
using SharpCompress.Archives.SevenZip;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileWrapper : ISevenZipFile
{
    public IGenericArchive OpenRead(string archiveFileName) =>
        new SevenZipArchiveWrapper(SevenZipArchive.Open(archiveFileName));
}