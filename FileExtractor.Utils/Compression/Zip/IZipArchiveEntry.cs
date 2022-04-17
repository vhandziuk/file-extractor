namespace FileExtractor.Utils.Compression.Zip;

public interface IZipArchiveEntry
{
    string Name { get; }
    string FullName { get; }

    void ExtractToFile(string destinationFileName, bool overwrite);
}