using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Application;

internal sealed class App : IApp
{
    private readonly string[] _supportedArchiveExtensions = { ".zip", ".rar" };

    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ICsvFileInfoProvider _fileInfoProvider;
    private readonly IArchiveExtractor _archiveExtractor;
    private readonly ILogger<App> _logger;

    public App(
        IFileSystemUtils fileSystemUtils,
        ICsvFileInfoProvider fileInfoProvider,
        IArchiveExtractor archiveExtractor,
        ILogger<App> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _fileInfoProvider = fileInfoProvider;
        _archiveExtractor = archiveExtractor;
        _logger = logger;
    }

    public async ValueTask RunAsync(ICommandLineOptions options)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var sourcePath = options.Source ?? baseDirectory;
        var destinationPath = options.Destination ?? sourcePath;
        var configurationPath = options.Configuration ?? Path.Combine(sourcePath, "configuration.csv");

        try
        {
            var archives = _fileSystemUtils
                .GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                .Where(filePath =>
                    _supportedArchiveExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase));

            if (!archives.Any())
            {
                _logger.Warning("Source directory contains no supported archive files. The program will now exit");
                return;
            }

            var fileData = _fileInfoProvider
                .EnumerateEntries(configurationPath);

            if (!fileData.Any())
            {
                _logger.Warning("Supplied configuration contains files to extract. The program will now exit");
                return;
            }

            _logger.Information("Starting file extraction");
            await _archiveExtractor.ExtractFiles(archives, destinationPath, fileData);
            _logger.Information("File exctaction completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An exception occurred. The program will now exit");
        }
    }
}