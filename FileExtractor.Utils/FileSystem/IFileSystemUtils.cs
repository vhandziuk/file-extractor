namespace FileExtractor.Utils.FileSystem;

public interface IFileSystemUtils
{
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    string[] GetFiles(string path);
}