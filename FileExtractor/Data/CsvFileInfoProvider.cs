namespace FileExtractor.Data;

internal sealed class CsvFileInfoProvider : ICsvFileInfoProvider
{
    public IEnumerable<FileInfoData> EnumerateEntries(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(stream))
        {
            while (reader.ReadLine() is string line)
            {
                var data = line.Split(',');
                yield return new FileInfoData(data[0], data[1]);
            }
        }
    }
}