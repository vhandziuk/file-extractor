namespace FileExtractor.Utils;

internal sealed class FileSystemUtils : IFileSystemUtils
{
    public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> EnumerateFiles(string path) => Directory.EnumerateFiles(path);

    public string GetCurrentDirectory() => Directory.GetCurrentDirectory();
}
