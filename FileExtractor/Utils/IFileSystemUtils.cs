namespace FileExtractor.Utils;

internal interface IFileSystemUtils
{
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    IEnumerable<string> EnumerateFiles(string path);
    string GetCurrentDirectory();
}