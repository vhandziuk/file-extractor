using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileUtils : ISevenZipFileUtils
{
    private readonly ISevenZipFile _sevenZipFile;

    public SevenZipFileUtils(ISevenZipFile sevenZipFile) =>
        _sevenZipFile = sevenZipFile;

    public IGenericArchive OpenRead(string archiveFileName) =>
        _sevenZipFile.OpenRead(archiveFileName);
}