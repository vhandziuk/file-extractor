using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public interface IArchiveExtractor
{
    Task<IEnumerable<FileInfoData>> ExtractFiles(IEnumerable<string> archiveFiles, string outputPath, IEnumerable<FileInfoData> fileData);
}