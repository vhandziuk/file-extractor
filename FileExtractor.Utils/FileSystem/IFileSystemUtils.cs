namespace FileExtractor.Utils.FileSystem;

public interface IFileSystemUtils
{
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    bool FileExists(string path);
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
}