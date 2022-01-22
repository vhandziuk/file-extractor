namespace FileExtractor
{
    internal interface IFileNameProvider
    {
        IAsyncEnumerable<FileInfoData> EnumerateFiles();
    }
}