namespace FileExtractor.Utils.Compression.Common;

public interface IGenericArchiveEntry
{
    string Name { get; }
    string FullName { get; }

    void ExtractToFile(string destinationFileName, bool overwrite);
}