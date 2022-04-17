namespace FileExtractor.Utils.Compression;

public interface IRarArchiveEntry
{
    string Name { get; }
    string FullName { get; }

    void ExtractToFile(string destinationFileName, bool overwrite);
}