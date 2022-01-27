namespace FileExtractor.Data;

internal interface ICsvFileNameProvider
{
    IEnumerable<FileInfoData> EnumerateFiles(string filePath);
}