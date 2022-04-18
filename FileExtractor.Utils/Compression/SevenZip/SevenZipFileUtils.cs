using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileUtils : GenericArchiveFileUtilsBase, ISevenZipFileUtils
{
    public SevenZipFileUtils(ISevenZipFile sevenZipFile)
        : base(sevenZipFile)
    {
    }
}