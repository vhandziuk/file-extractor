namespace FileExtractor.Utils.Compression;

public interface IArchiveExtractorFactory
{
    IArchiveFileExtractor Create(ArchiveType archiveType);
}
