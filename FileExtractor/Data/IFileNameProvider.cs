namespace FileExtractor.Data;

internal interface IFileNameProvider
{
    IAsyncEnumerable<FileInfoData> EnumerateFiles();
}