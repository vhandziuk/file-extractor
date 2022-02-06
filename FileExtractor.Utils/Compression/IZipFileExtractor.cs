using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public interface IZipFileExtractor
{
    Task ExtractFiles(IEnumerable<string> archives, string outputPath, IEnumerable<FileInfoData> fileData);
}