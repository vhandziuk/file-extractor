namespace FileExtractor.Utils.Compression.Rar;

public interface IRarArchiveEntry
{
    string Name { get; }
    string FullName { get; }

    void ExtractToFile(string destinationFileName, bool overwrite);
}