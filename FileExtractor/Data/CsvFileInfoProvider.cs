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
                if (data.Length != 2)
                {
                    throw new Exception(
                        "Malformed configuration file. " +
                        "Data must contain 2 columns containing file name " +
                        "and [optionally] subfolder path or an empty space separated by a comma");
                }

                yield return new FileInfoData(data[0], data[1]);
            }
        }
    }
}