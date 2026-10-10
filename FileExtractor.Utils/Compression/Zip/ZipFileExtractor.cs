using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Zip;

public sealed class ZipFileExtractor(
    IZipFileUtils zipFileUtils,
    IArchiveEntryExtractor archiveEntryExtractor)
    : GenericArchiveExtractorBase(zipFileUtils, archiveEntryExtractor), IZipFileExtractor
{
}
