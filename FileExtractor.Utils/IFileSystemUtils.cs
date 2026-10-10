namespace FileExtractor.Utils;

public interface IFileSystemUtils
{
    bool DirectoryExists(string path);
    DirectoryInfo CreateDirectory(string path);
    bool FileExists(string path);
    long GetFileSize(string path);
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
    void Copy(string sourceFileName, string destFileName, bool overwrite);
    void DeleteFile(string path);
    bool IsReparsePoint(string path);
}
