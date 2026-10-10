namespace FileExtractor.Data;

public sealed class CsvFileInfoProvider(ITextFile textFile) : ICsvFileInfoProvider
{
    private const int RequiredColumnCount = 3;

    public IEnumerable<FileInfoData> EnumerateEntries(string filePath)
    {
        var lineNumber = 0;

        foreach (var line in textFile.ReadLines(filePath))
        {
            lineNumber++;
            yield return Parse(line, lineNumber);
        }
    }

    private static FileInfoData Parse(string line, int lineNumber)
    {
        var data = line.Split(',');
        if (data.Length != RequiredColumnCount)
        {
            throw new InvalidDataException(
                $"Malformed configuration file at line {lineNumber}. " +
                "Data must contain 3 columns containing file name, " +
                "extraction subfolder, and path in the archive");
        }

        var name = data[0];
        var directory = data[1];
        var location = data[2];

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDataException($"File name cannot be empty or whitespace (line {lineNumber})");
        }

        return new FileInfoData(directory, name, location);
    }
}
