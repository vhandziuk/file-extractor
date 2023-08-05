namespace FileExtractor.Data;

public sealed class CsvFileInfoProvider : ICsvFileInfoProvider
{
    private static readonly HashSet<int> AllowedColumnCounts = new() { 1, 2, 3 };

    public IEnumerable<FileInfoData> EnumerateEntries(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is string line)
        {
            var data = line.Split(',');
            if (!AllowedColumnCounts.Contains(data.Length))
            {
                throw new Exception(
                    "Malformed configuration file. " +
                    "Data must contain 1, 2, or 3 columns containing [required] file name, [optional] extraction" +
                    "subfolder, and [optional] archive path (subfolder in which the file is located)");
            }

            var name = data[0];
            var directory = data.ElementAtOrDefault(1) ?? string.Empty;
            var location = data.ElementAtOrDefault(2) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("File name cannot be empty or whitespace");
            }

            yield return new FileInfoData(directory, name, location);
        }
    }
}