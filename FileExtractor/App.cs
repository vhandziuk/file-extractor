using System.Reflection;
using FileExtractor.Data;
using FileExtractor.Utils;

namespace FileExtractor;

internal sealed class App : IApp
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ICsvFileInfoProvider _fileInfoProvider;
    private readonly IZipFileExtractor _zipFileExtractor;
    private readonly ILogger<App> _logger;

    public App(
        IFileSystemUtils fileSystemUtils,
        ICsvFileInfoProvider fileInfoProvider,
        IZipFileExtractor zipFileExtractor,
        ILogger<App> logger)
    {
        _fileSystemUtils = fileSystemUtils;
        _fileInfoProvider = fileInfoProvider;
        _zipFileExtractor = zipFileExtractor;
        _logger = logger;
    }

    public async ValueTask RunAsync(ICommandLineOptions options)
    {
        var executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
            ?? Directory.GetCurrentDirectory();

        var sourcePath = options.Source ?? executablePath;
        var destinationPath = options.Destination ?? sourcePath;
        var configurationPath = options.Configuration ?? Path.Combine(sourcePath, "configuration.csv");

        try
        {
            var archives = _fileSystemUtils
                .GetFiles(sourcePath)
                .Where(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            if (!archives.Any())
            {
                _logger.Warning("Source directory contains no .zip files. The program will now exit");
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
            await _zipFileExtractor.ExtractFiles(archives, destinationPath, fileData);
            _logger.Information("File exctaction completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An exception occurred. The program will now exit");
        }
    }
}
