using FileExtractor.Utils.Compression.Other;
using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.Compression.SevenZip;
using FileExtractor.Utils.Compression.Zip;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractorFactory(
    IZipFileExtractor zipFileExtractor,
    IRarFileExtractor rarFileExtractor,
    ISevenZipFileExtractor sevenZipFileExtractor,
    IOtherArchiveFileExtractor otherArchiveFileExtractor) : IArchiveExtractorFactory
{
    public IArchiveFileExtractor Create(ArchiveType archiveType) =>
        archiveType switch
        {
            ArchiveType.Zip => zipFileExtractor,
            ArchiveType.Rar => rarFileExtractor,
            ArchiveType.SevenZip => sevenZipFileExtractor,
            ArchiveType.Other => otherArchiveFileExtractor,
            _ => throw new ArgumentOutOfRangeException(nameof(archiveType), archiveType, "Unsupported archive type")
        };
}
