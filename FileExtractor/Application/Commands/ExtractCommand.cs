using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;

namespace FileExtractor.Application.Commands;

internal sealed class ExtractCommand(
    IEnvironment environment,
    IFileSystemUtils fileSystemUtils,
    IFilePatternParser filePatternParser,
    IArchiveFinder archiveFinder,
    IArchiveExtractor archiveExtractor,
    IExtractionSettingsFactory extractionSettingsFactory,
    ILogger<ExtractCommand> logger) : IExtractCommand
{
    private const string AllFilesPattern = "*";

    public async Task RunAsync(IExtractOptions options, CancellationToken cancellationToken)
    {
        // parsed first, so a mistyped pattern fails before anything is written
        var files = ParsePatterns(options.Files.DefaultIfEmpty(AllFilesPattern));
        var excludedFiles = ParsePatterns(options.Exclude);

        var archives = archiveFinder.FindArchives(options.Archive, options.Recursive);
        if (archives.Length == 0)
        {
            logger.Warning("No supported archive files found at {Path}. The program will now exit", options.Archive);
            return;
        }

        var outputPath = Path.GetFullPath(options.Output ?? environment.CurrentDirectory);
        CreateOutputDirectory(outputPath);

        var settings = extractionSettingsFactory.Create(options, outputPath, files) with
        {
            ExcludedFiles = excludedFiles,
            PreserveEntryPaths = !options.Flat,
            UseArchiveFolders = options.ArchiveFolder
        };

        logger.Information("Starting file extraction");
        await archiveExtractor.ExtractFilesAsync(archives, settings, cancellationToken);
        logger.Information("File extraction completed");
    }

    private FileInfoData[] ParsePatterns(IEnumerable<string> patterns) =>
        [.. patterns.Select(filePatternParser.Parse).Distinct()];

    private void CreateOutputDirectory(string outputPath)
    {
        if (fileSystemUtils.DirectoryExists(outputPath))
            return;

        logger.Information("Creating output directory {Path}", outputPath);
        fileSystemUtils.CreateDirectory(outputPath);
    }
}
