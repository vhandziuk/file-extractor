namespace FileExtractor.Data;

public interface ICsvFileInfoProvider
{
    IEnumerable<FileInfoData> EnumerateEntries(string filePath);
}