namespace FileExtractor.Data;

internal interface ICsvFileNameProvider
{
    IAsyncEnumerable<FileInfoData> EnumerateFiles(string filePath);
}