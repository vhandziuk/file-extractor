using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;
using static System.Environment;

namespace FileExtractor.Application;

internal sealed class App : IApp
{
    private static readonly HashSet<string> SupportedArchiveExtensions = new() { ".zip", ".rar", ".7z", ".tar", ".bz2", ".gz", ".lz", ".xz" };

    private readonly IEnvironment _environment;
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ICsvFileInfoProvider _fileInfoProvider;
    private readonly IArchiveExtractor _archiveExtractor;
    private readonly ILogger<App> _logger;

    public App(
        IEnvironment environment,
        IFileSystemUtils fileSystemUtils,
        ICsvFileInfoProvider fileInfoProvider,
        IArchiveExtractor archiveExtractor,
        ILogger<App> logger)
    {
        _environment = environment;
        _fileSystemUtils = fileSystemUtils;
        _fileInfoProvider = fileInfoProvider;
        _archiveExtractor = archiveExtractor;
        _logger = logger;
    }

    public async ValueTask RunAsync(ICommandLineOptions options)
    {
        try
        {
            var sourcePath = options.Source != null && _fileSystemUtils.DirectoryExists(options.Source)
                ? options.Source
                : null;

            if (sourcePath == null)
            {
                _logger.Warning("Source path is not provided or does not exist. The program will now exit");
                return;
            }

            var destinationPath = options.Destination != null && _fileSystemUtils.DirectoryExists(options.Destination)
                ? options.Destination
                : sourcePath;

            var defaultConfigurationLocation = Path.Combine(sourcePath, "configuration.csv");
            var cachedConfigurationLocation = Path.Combine(
        _environment.GetFolderPath(SpecialFolder.CommonApplicationData), "File Extractor", "configuration.csv");
            var configurationPath = options.Configuration != null && _fileSystemUtils.FileExists(options.Configuration)
                ? options.Configuration
                : _fileSystemUtils.FileExists(defaultConfigurationLocation)
                    ? defaultConfigurationLocation
                    : _fileSystemUtils.FileExists(cachedConfigurationLocation)
                        ? cachedConfigurationLocation
                        : null;

            if (configurationPath == null)
            {
                _logger.Warning("Unable to locate the configuration file. The program will now exit");
                return;
            }

            var archives = _fileSystemUtils
                .GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                .Where(filePath =>
                    SupportedArchiveExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase));

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

            if (options.CacheConfiguration && configurationPath != cachedConfigurationLocation)
            {
                _fileSystemUtils.Copy(configurationPath, cachedConfigurationLocation, true);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An exception occurred. The program will now exit");
        }
    }
}