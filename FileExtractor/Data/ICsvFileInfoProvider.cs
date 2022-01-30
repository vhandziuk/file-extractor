namespace FileExtractor.Data;

internal interface ICsvFileInfoProvider
{
    IEnumerable<FileInfoData> EnumerateEntries(string filePath);
}