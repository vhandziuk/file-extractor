namespace FileExtractor.Utils.Compression;

public interface IArchiveEntry
{
    string Name { get; }
    string FullName { get; }

    void ExtractToFile(string destinationFileName, bool overwrite);
}