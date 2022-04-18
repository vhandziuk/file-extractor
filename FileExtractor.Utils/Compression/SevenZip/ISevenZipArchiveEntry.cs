namespace FileExtractor.Utils.Compression.SevenZip;

public interface ISevenZipArchiveEntry
{
    string Name { get; }
    string FullName { get; }

    void ExtractToFile(string destinationFileName, bool overwrite);
}