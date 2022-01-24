using System.Collections.Immutable;
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

    public async ValueTask Run(string sourcePath, string destinationPath, string configurationPath)
    {
        var archives = _fileSystemUtils
            .EnumerateFiles(sourcePath)
            .Where(x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            .ToImmutableArray();

        await _zipFileExtractor.ExtractFiles(
                archives,
                destinationPath,
                _fileNameProvider.EnumerateFiles(configurationPath));
    }
}
