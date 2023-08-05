namespace FileExtractor.Data;

public sealed class CsvFileInfoProvider : ICsvFileInfoProvider
{
    private const int RequiredColumnCount = 3;

    public IEnumerable<FileInfoData> EnumerateEntries(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is string line)
        {
            var data = line.Split(',');
            if (data.Length != RequiredColumnCount)
            {
                throw new Exception(
                    "Malformed configuration file. " +
                    "Data must contain 3 columns containing file name, " +
                    "extraction subfolder, and path in the archive");
            }

            var name = data[0];
            var directory = data[1];
            var location = data[2];

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("File name cannot be empty or whitespace");
            }

            yield return new FileInfoData(directory, name, location);
        }
    }
}