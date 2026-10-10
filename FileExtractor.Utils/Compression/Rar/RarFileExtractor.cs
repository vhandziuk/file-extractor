using FileExtractor.Utils.Compression.Common;

namespace FileExtractor.Utils.Compression.Rar;

public sealed class RarFileExtractor(
    IRarFileUtils rarFileUtils,
    IArchiveEntryExtractor archiveEntryExtractor)
    : GenericArchiveExtractorBase(rarFileUtils, archiveEntryExtractor), IRarFileExtractor
{
}
