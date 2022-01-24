using FileExtractor.Data;

namespace FileExtractor.Utils;

internal interface IZipFileExtractor
{
    ValueTask ExtractFiles(IEnumerable<string> archives, string outputPath, IAsyncEnumerable<FileInfoData> fileData);
}