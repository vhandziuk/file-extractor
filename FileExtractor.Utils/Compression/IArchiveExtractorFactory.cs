namespace FileExtractor.Utils.Compression;

public interface IArchiveExtractorFactory
{
    IArchiveExtractor Create(ArchiveType archiveType);
}