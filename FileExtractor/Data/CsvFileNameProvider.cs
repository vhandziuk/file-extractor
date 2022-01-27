namespace FileExtractor.Data;

internal sealed class CsvFileNameProvider : ICsvFileNameProvider
{
    public IEnumerable<FileInfoData> EnumerateFiles(string filePath)
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