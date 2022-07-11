using FileExtractor.Common.Logging;

namespace FileExtractor.Utils.FileSystem;

public sealed class FileSystemUtils : IFileSystemUtils
{
    private readonly ILogger<FileSystemUtils> _logger;

    public FileSystemUtils(ILogger<FileSystemUtils> logger)
    {
        _logger = logger;
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        try
        {
            return Directory.CreateDirectory(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create directory: {Path}", path);
            throw;
        }
    }

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        try
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get files from directory: {Path}", path);
            throw;
        }
    }

    public void Copy(string sourceFileName, string destFileName, bool overwrite)
    {
        try
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to copy file from {SourceFileName} to {DestFileName}", sourceFileName, destFileName);
        }
    }

    public string GetAppBaseDirectory() => AppDomain.CurrentDomain.BaseDirectory;
}