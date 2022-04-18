using SharpCompress.Archives.SevenZip;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileWrapper : ISevenZipFile
{
    public ISevenZipArchive OpenRead(string archiveFileName) =>
        new SevenZipArchiveWrapper(SevenZipArchive.Open(archiveFileName));
}