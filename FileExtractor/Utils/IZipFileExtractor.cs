using FileExtractor.Data;

namespace FileExtractor.Utils;

internal interface IZipFileExtractor
{
    Task ExtractFiles(IEnumerable<string> archives, IAsyncEnumerable<FileInfoData> fileData, string outputPath);
}