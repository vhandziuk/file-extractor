namespace FileExtractor.Utils.FileSystem;

public interface IFileSystemUtils
{
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
}