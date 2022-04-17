namespace FileExtractor.Data;

public sealed class CsvFileInfoProvider : ICsvFileInfoProvider
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

                var directory = Path.GetDirectoryName(data[0]) ?? string.Empty;
                var name = Path.GetFileName(data[0]);
                var location = data[1];

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("File name cannot be empty or whitespace");
                }

                yield return new FileInfoData(directory, name, location);
            }
        }
    }
}