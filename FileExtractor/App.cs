using System.Reflection;
using FileExtractor.Data;
using FileExtractor.Utils;

namespace FileExtractor;

internal sealed class App : IApp
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ICsvFileInfoProvider _fileInfoProvider;
    private readonly IZipFileExtractor _zipFileExtractor;

    public App(
        IFileSystemUtils fileSystemUtils,
        ICsvFileInfoProvider fileInfoProvider,
        IZipFileExtractor zipFileExtractor)
    {
        _fileSystemUtils = fileSystemUtils;
        _fileInfoProvider = fileInfoProvider;
        _zipFileExtractor = zipFileExtractor;
    }

    public async ValueTask RunAsync(ICommandLineOptions options)
    {
        var executablePath = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();

        var sourcePath = options.Source ?? executablePath;
        var destinationPath = options.Destination ?? sourcePath;
        var configurationPath = options.Configuration ?? Path.Combine(sourcePath, "configuration.csv");

        var archives = _fileSystemUtils
            .GetFiles(sourcePath)
            .Where(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        if (!archives.Any())
            return;

        await _zipFileExtractor.ExtractFiles(
                archives,
                destinationPath,
                _fileInfoProvider.EnumerateEntries(configurationPath));
    }
}
