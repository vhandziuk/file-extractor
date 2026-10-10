using FileExtractor.Common.Logging;

namespace FileExtractor.Utils;

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

    public long GetFileSize(string path) => new FileInfo(path).Length;

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        try
        {
            // An inaccessible subfolder must not abort the whole scan. Win32 matching keeps "*.*"
            // matching every file, as the SearchOption overload does.
            return Directory.GetFiles(
                path,
                searchPattern,
                new EnumerationOptions
                {
                    RecurseSubdirectories = searchOption == SearchOption.AllDirectories,
                    IgnoreInaccessible = true,
                    MatchType = MatchType.Win32
                });
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
            throw;
        }
    }

    public void DeleteFile(string path) => File.Delete(path);
}
