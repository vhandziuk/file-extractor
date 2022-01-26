using FileExtractor.Data;

namespace FileExtractor.Utils;

internal interface IZipFileExtractor
{
    Task ExtractFiles(IEnumerable<string> archives, string outputPath, IAsyncEnumerable<FileInfoData> fileData);
}