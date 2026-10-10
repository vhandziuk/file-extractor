using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;

namespace FileExtractor.Application.Commands;

internal sealed class ExtractCommand(
    IEnvironment environment,
    IFilePatternParser filePatternParser,
    IArchiveFinder archiveFinder,
    IArchiveExtractor archiveExtractor,
    IExtractionSettingsFactory extractionSettingsFactory,
    IOutputPathResolver outputPathResolver,
    ILogger<ExtractCommand> logger) : IExtractCommand
{
    private const string AllFilesPattern = "*";
    private const char PatternListSeparator = ',';

    public async Task RunAsync(IExtractOptions options, CancellationToken cancellationToken)
    {
        // parsed first, so a mistyped pattern fails before anything is written
        var files = ParsePatterns(options.Files.DefaultIfEmpty(AllFilesPattern));
        var excludedFiles = ParsePatterns(SplitPatternList(options.Exclude));

        var archives = archiveFinder.FindArchives(options.Archive, options.Recursive);
        if (archives.Length == 0)
        {
            logger.Warning("No supported archive files found at {Path}. The program will now exit", options.Archive);
            return;
        }

        var outputPath = outputPathResolver.Resolve(options.Output, environment.CurrentDirectory);

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

    private static string[] SplitPatternList(string? patternList) =>
        patternList?.Split(PatternListSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    private FileInfoData[] ParsePatterns(IEnumerable<string> patterns) =>
        [.. patterns.Select(filePatternParser.Parse).Distinct()];
}
