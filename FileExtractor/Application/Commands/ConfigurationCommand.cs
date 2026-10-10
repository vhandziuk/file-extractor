using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;
using static System.Environment;

namespace FileExtractor.Application.Commands;

internal sealed class ConfigurationCommand(
    IEnvironment environment,
    IFileSystemUtils fileSystemUtils,
    ICsvFileInfoProvider fileInfoProvider,
    IArchiveFinder archiveFinder,
    IArchiveExtractor archiveExtractor,
    IExtractionSettingsFactory extractionSettingsFactory,
    IOutputPathResolver outputPathResolver,
    IReparsePointGuard reparsePointGuard,
    ILogger<ConfigurationCommand> logger) : IConfigurationCommand
{
    private const string ConfigurationFileName = "configuration.csv";

    public async Task RunAsync(IConfigurationOptions options, CancellationToken cancellationToken)
    {
        var sourcePath = options.Source;
        if (!fileSystemUtils.DirectoryExists(sourcePath))
        {
            logger.Warning("Source path {Path} does not exist. The program will now exit", sourcePath);
            return;
        }

        var cachedConfigurationPath = GetCachedConfigurationPath();
        var configurationPath = FindConfigurationPath(options.Configuration, sourcePath, cachedConfigurationPath);
        if (configurationPath is null)
        {
            logger.Warning("Unable to locate the configuration file. The program will now exit");
            return;
        }

        var archives = archiveFinder.FindArchives(sourcePath, options.Recursive);
        if (archives.Length == 0)
        {
            logger.Warning("Source directory contains no supported archive files. The program will now exit");
            return;
        }

        // read the configuration once; every archive is matched against the same list
        var fileData = fileInfoProvider
            .EnumerateEntries(configurationPath)
            .Distinct()
            .ToArray();
        if (fileData.Length == 0)
        {
            logger.Warning("Supplied configuration contains no files to extract. The program will now exit");
            return;
        }

        var outputPath = outputPathResolver.Resolve(options.Output, sourcePath);
        var settings = extractionSettingsFactory.Create(options, outputPath, fileData);

        logger.Information("Starting file extraction");
        await archiveExtractor.ExtractFilesAsync(archives, settings, cancellationToken);
        logger.Information("File extraction completed");

        if (options.CacheConfiguration)
            CacheConfiguration(configurationPath, cachedConfigurationPath);
    }

    // Cached per user: in a folder shared by all users, such as ProgramData, another user could plant the
    // configuration used by default or a junction that redirects the copy of an elevated run.
    private string GetCachedConfigurationPath() =>
        Path.Combine(GetLocalApplicationDataPath(), "File Extractor", ConfigurationFileName);

    private string GetLocalApplicationDataPath() =>
        environment.GetFolderPath(SpecialFolder.LocalApplicationData);

    private string? FindConfigurationPath(string? configuredPath, string sourcePath, string cachedConfigurationPath)
    {
        string?[] candidatePaths = [configuredPath, Path.Combine(sourcePath, ConfigurationFileName), cachedConfigurationPath];

        return candidatePaths.FirstOrDefault(path => path is not null && fileSystemUtils.FileExists(path));
    }

    private void CacheConfiguration(string configurationPath, string cachedConfigurationPath)
    {
        if (string.Equals(configurationPath, cachedConfigurationPath, StringComparison.OrdinalIgnoreCase))
            return;

        // CreateDirectory and Copy would follow a junction or symbolic link to an arbitrary location
        if (reparsePointGuard.ContainsReparsePoint(GetLocalApplicationDataPath(), cachedConfigurationPath))
        {
            logger.Warning("Cached configuration path {Path} contains a symbolic link or junction. The configuration is not cached", cachedConfigurationPath);
            return;
        }

        fileSystemUtils.CreateDirectory(Path.GetDirectoryName(cachedConfigurationPath)!);
        fileSystemUtils.Copy(configurationPath, cachedConfigurationPath, true);
    }
}
