using FileExtractor.Data;
using FileExtractor.Utils;

namespace FileExtractor;

internal sealed class ExtractFlow : IExtractFlow
{
    private readonly IFileSystemUtils _fileSystemUtils;
    private readonly ICsvFileNameProvider _fileNameProvider;
    private readonly IZipFileExtractor _zipFileExtractor;

    public ExtractFlow(
        IFileSystemUtils fileSystemUtils,
        ICsvFileNameProvider fileNameProvider,
        IZipFileExtractor zipFileExtractor)
    {
        _fileSystemUtils = fileSystemUtils;
        _fileNameProvider = fileNameProvider;
        _zipFileExtractor = zipFileExtractor;
    }

    public async ValueTask Extract(string sourcePath, string destinationPath, string configurationPath)
    {
        await _zipFileExtractor.ExtractFiles(
                _fileSystemUtils
                    .EnumerateFiles(sourcePath)
                    .Where(x => x.EndsWith(".zip")),
                _fileNameProvider.EnumerateFiles(configurationPath),
                destinationPath);
    }
}
