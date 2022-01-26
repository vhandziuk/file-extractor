using FileExtractor.Data;
using FileExtractor.Utils;

namespace FileExtractor;

internal sealed class App : IApp
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ICsvFileNameProvider _fileNameProvider;
    private readonly IZipFileExtractor _zipFileExtractor;

    public App(
        IFileSystemUtils fileSystemUtils,
        ICsvFileNameProvider fileNameProvider,
        IZipFileExtractor zipFileExtractor)
    {
        _fileSystemUtils = fileSystemUtils;
        _fileNameProvider = fileNameProvider;
        _zipFileExtractor = zipFileExtractor;
    }

    public async Task RunAsync(string sourcePath, string destinationPath, string configurationPath)
    {
        var archives = _fileSystemUtils
            .GetFiles(sourcePath)
            .Where(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        await _zipFileExtractor.ExtractFiles(
                archives,
                destinationPath,
                _fileNameProvider.EnumerateFiles(configurationPath));
    }
}
