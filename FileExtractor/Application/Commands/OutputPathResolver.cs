using FileExtractor.Common.Logging;
using FileExtractor.Utils;

namespace FileExtractor.Application.Commands;

internal sealed class OutputPathResolver(
    IFileSystemUtils fileSystemUtils,
    ILogger<OutputPathResolver> logger) : IOutputPathResolver
{
    public string Resolve(string? outputPath, string defaultPath)
    {
        var fullPath = Path.GetFullPath(outputPath ?? defaultPath);
        if (fileSystemUtils.DirectoryExists(fullPath))
            return fullPath;

        logger.Information("Creating output directory {Path}", fullPath);
        fileSystemUtils.CreateDirectory(fullPath);

        return fullPath;
    }
}
