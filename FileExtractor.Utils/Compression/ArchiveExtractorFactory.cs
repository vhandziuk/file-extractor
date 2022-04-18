using FileExtractor.Utils.Compression.Other;
using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.Compression.SevenZip;
using FileExtractor.Utils.Compression.Zip;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractorFactory : IArchiveExtractorFactory
{
    private readonly IZipFileExtractor _zipFileExtractor;
    private readonly IRarFileExtractor _rarFileExtractor;
    private readonly ISevenZipFileExtractor _sevenZipFileExtractor;
    private readonly IOtherArchiveFileExtractor _otherArchiveFileExtractor;

    public ArchiveExtractorFactory(
        IZipFileExtractor zipFileExtractor,
        IRarFileExtractor rarFileExtractor,
        ISevenZipFileExtractor sevenZipFileExtractor,
        IOtherArchiveFileExtractor otherArchiveFileExtractor)
    {
        _zipFileExtractor = zipFileExtractor;
        _rarFileExtractor = rarFileExtractor;
        _sevenZipFileExtractor = sevenZipFileExtractor;
        _otherArchiveFileExtractor = otherArchiveFileExtractor;
    }

    public IArchiveExtractor Create(ArchiveType archiveType) =>
        archiveType switch
        {
            ArchiveType.Zip => _zipFileExtractor,
            ArchiveType.Rar => _rarFileExtractor,
            ArchiveType.SevenZip => _sevenZipFileExtractor,
            ArchiveType.Other => _otherArchiveFileExtractor,
            _ => throw new Exception("Unsupported archive type")
        };
}