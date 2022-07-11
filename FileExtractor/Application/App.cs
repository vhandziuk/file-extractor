using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.FileSystem;

namespace FileExtractor.Application;

internal sealed class App : IApp
{
    private readonly string[] _supportedArchiveExtensions =
    {
        ".zip",
        ".rar",
        ".7z",
        ".tar",
        ".bz2",
        ".gz",
        ".lz",
        ".xz"
    };

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
        var baseDirectory = _fileSystemUtils.GetAppBaseDirectory();

        var sourcePath = options.Source != null && _fileSystemUtils.DirectoryExists(options.Source)
            ? options.Source
            : baseDirectory;
        var destinationPath = options.Destination != null && _fileSystemUtils.DirectoryExists(options.Destination)
            ? options.Destination
            : sourcePath;

        try
        {
            var defaultConfigurationLocation = Path.Combine(sourcePath, "configuration.csv");
            var cachedConfigurationLocation = Path.Combine(baseDirectory, "configuration.csv");
            var configurationPath = options.Configuration != null && _fileSystemUtils.FileExists(options.Configuration)
                ? options.Configuration
                : _fileSystemUtils.FileExists(defaultConfigurationLocation)
                    ? defaultConfigurationLocation
                    : _fileSystemUtils.FileExists(cachedConfigurationLocation)
                        ? cachedConfigurationLocation
                        : null;

            if (configurationPath is null)
            {
                _logger.Warning("Unable to locate the configuration file. The program will now exit");
                return;
            }

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
                _logger.Warning("Supplied configuration contains no files to extract. The program will now exit");
                return;
            }

            _logger.Information("Starting file extraction");
            await _archiveExtractor.ExtractFiles(archives, destinationPath, fileData);
            _logger.Information("File extraction completed");

            if (options.CacheConfiguration)
            {
                _fileSystemUtils.Copy(configurationPath, Path.Combine(baseDirectory, "configuration.csv"), true);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An exception occurred. The program will now exit");
        }
    }
}