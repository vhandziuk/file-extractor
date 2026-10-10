using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.SevenZip;

public sealed class SevenZipFileExtractor(
    ISevenZipFileUtils sevenZipFileUtils,
    IArchiveEntryExtractor archiveEntryExtractor)
    : GenericArchiveExtractorBase(sevenZipFileUtils, archiveEntryExtractor), ISevenZipFileExtractor
{
}
