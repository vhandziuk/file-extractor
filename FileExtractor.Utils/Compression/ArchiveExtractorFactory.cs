using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.Compression.Zip;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractorFactory : IArchiveExtractorFactory
{
    private readonly IZipFileExtractor _zipFileExtractor;
    private readonly IRarFileExtractor _rarFileExtractor;

    public ArchiveExtractorFactory(
        IZipFileExtractor zipFileExtractor,
        IRarFileExtractor rarFileExtractor)
    {
        _zipFileExtractor = zipFileExtractor;
        _rarFileExtractor = rarFileExtractor;
    }

    public IArchiveExtractor Create(ArchiveType archiveType) =>
        archiveType switch
        {
            ArchiveType.Zip => _zipFileExtractor,
            ArchiveType.Rar => _rarFileExtractor,
            _ => throw new Exception("Unsupported archive type")
        };
}