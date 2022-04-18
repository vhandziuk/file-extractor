using FileExtractor.Utils.Compression.Other;
using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.Compression.Zip;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractorFactory : IArchiveExtractorFactory
{
    private readonly IZipFileExtractor _zipFileExtractor;
    private readonly IRarFileExtractor _rarFileExtractor;
    private readonly IOtherArchiveFileExtractor _otherArchiveFileExtractor;

    public ArchiveExtractorFactory(
        IZipFileExtractor zipFileExtractor,
        IRarFileExtractor rarFileExtractor,
        IOtherArchiveFileExtractor otherArchiveFileExtractor)
    {
        _zipFileExtractor = zipFileExtractor;
        _rarFileExtractor = rarFileExtractor;
        _otherArchiveFileExtractor = otherArchiveFileExtractor;
    }

    public IArchiveExtractor Create(ArchiveType archiveType) =>
        archiveType switch
        {
            ArchiveType.Zip => _zipFileExtractor,
            ArchiveType.Rar => _rarFileExtractor,
            ArchiveType.Other => _otherArchiveFileExtractor,
            _ => throw new Exception("Unsupported archive type")
        };
}