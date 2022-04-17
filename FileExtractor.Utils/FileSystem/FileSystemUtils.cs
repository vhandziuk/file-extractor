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

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        try
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get files from directory: {Path}", path);
            throw;
        }
    }
}