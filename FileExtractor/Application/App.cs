using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;
using static System.Environment;

namespace FileExtractor.Application;

internal sealed class App(
    IEnvironment environment,
    IFileSystemUtils fileSystemUtils,
    ICsvFileInfoProvider fileInfoProvider,
    IArchiveTypeResolver archiveTypeResolver,
    IArchiveExtractor archiveExtractor,
    IReparsePointGuard reparsePointGuard,
    ILogger<App> logger) : IApp
{
    private const string ConfigurationFileName = "configuration.csv";

    public async ValueTask RunAsync(ICommandLineOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await ExtractAsync(options, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.Warning("Extraction was cancelled. The program will now exit");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An exception occurred. The program will now exit");
        }
    }

    private async Task ExtractAsync(ICommandLineOptions options, CancellationToken cancellationToken)
    {
        if (options.Source is not { } sourcePath || !fileSystemUtils.DirectoryExists(sourcePath))
        {
            logger.Warning("Source path is not provided or does not exist. The program will now exit");
            return;
        }

        var cachedConfigurationPath = GetCachedConfigurationPath();
        var configurationPath = FindConfigurationPath(options.Configuration, sourcePath, cachedConfigurationPath);
        if (configurationPath is null)
        {
            logger.Warning("Unable to locate the configuration file. The program will now exit");
            return;
        }

        var archives = FindArchives(sourcePath, options.Recursive);
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

        if (options.FollowSymbolicLinks)
            logger.Warning("Symbolic links and junctions below the destination path are followed");

        logger.Information("Starting file extraction");
        await archiveExtractor.ExtractFilesAsync(
            archives,
            GetDestinationPath(options.Destination, sourcePath),
            fileData,
            GetMaxDegreeOfParallelism(options.MaxParallelism),
            options.FollowSymbolicLinks,
            cancellationToken);
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

    private string[] FindArchives(string sourcePath, bool recursive) =>
        fileSystemUtils
            .GetFiles(sourcePath, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(filePath => archiveTypeResolver.TryResolve(filePath, out _))
            .ToArray();

    private string GetDestinationPath(string? destinationPath, string sourcePath) =>
        destinationPath is not null && fileSystemUtils.DirectoryExists(destinationPath)
            ? destinationPath
            : sourcePath;

    private int GetMaxDegreeOfParallelism(int? maxParallelism)
    {
        if (maxParallelism is { } value and > 0)
            return value;

        if (maxParallelism is not null)
            logger.Warning("Maximum parallelism must be a positive number. Using {ProcessorCount} instead", environment.ProcessorCount);

        return environment.ProcessorCount;
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
