namespace FileExtractor.Data;

internal sealed class CsvFileNameProvider : ICsvFileNameProvider
{
    public async IAsyncEnumerable<FileInfoData> EnumerateFiles(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(stream))
        {
            while (await reader.ReadLineAsync() is string line)
            {
                var data = line.Split(',');
                yield return new FileInfoData(data[0], data[1]);
            }
        }
    }
}