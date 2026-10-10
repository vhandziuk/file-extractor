using System.Collections.Concurrent;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;

namespace FileExtractor.Utils.Compression;

public sealed class ArchiveExtractor(
    IArchiveExtractorFactory archiveExtractorFactory,
    IArchiveTypeResolver archiveTypeResolver,
    IFileSystemUtils fileSystemUtils,
    ITaskRunner taskRunner,
    ILogger<ArchiveExtractor> logger) : IArchiveExtractor
{
    public async Task<IReadOnlyCollection<FileInfoData>> ExtractFilesAsync(
        IReadOnlyCollection<string> archiveFiles,
        string outputPath,
        IReadOnlyList<FileInfoData> fileData,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken)
    {
        logger.Information("Processing files");

        var context = new ExtractionContext(outputPath, fileData);
        var extractedFiles = new ConcurrentBag<FileInfoData>();

        await taskRunner.ForEachAsync(
            OrderByLargestFirst(archiveFiles),
            maxDegreeOfParallelism,
            (archiveFile, token) =>
            {
                foreach (var fileInfo in ExtractArchive(archiveFile, context, token))
                    extractedFiles.Add(fileInfo);

                return ValueTask.CompletedTask;
            },
            cancellationToken);

        var result = extractedFiles.ToHashSet();
        ReportMissingFiles(fileData, result);

        return result;
    }

    // Starting with the largest archives keeps a long-running archive from being picked up last,
    // while the other workers would already be idle.
    private IEnumerable<string> OrderByLargestFirst(IEnumerable<string> archiveFiles) =>
        archiveFiles.OrderByDescending(fileSystemUtils.GetFileSize);

    private IReadOnlyCollection<FileInfoData> ExtractArchive(string archiveFile, IExtractionContext context, CancellationToken cancellationToken)
    {
        if (!archiveTypeResolver.TryResolve(archiveFile, out var archiveType))
        {
            logger.Warning("Archive {Archive} has an unsupported type. Skipping it", archiveFile);
            return [];
        }

        try
        {
            return archiveExtractorFactory
                .Create(archiveType)
                .ExtractFiles(archiveFile, context, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // one corrupt or locked archive must not stop the others; its files are reported as missing
            logger.Error(ex, "Failed to extract files from {Archive}. Skipping it", archiveFile);
            return [];
        }
    }

    private void ReportMissingFiles(IEnumerable<FileInfoData> fileData, IReadOnlySet<FileInfoData> extractedFiles)
    {
        var missingFiles = fileData
            .Where(fileInfo => !extractedFiles.Contains(fileInfo))
            .OrderBy(fileInfo => fileInfo.Directory)
            .ThenBy(fileInfo => fileInfo.Name)
            .ToArray();

        if (missingFiles.Length == 0)
        {
            logger.Information("Processing completed. All files have been successfully extracted");
            return;
        }

        logger.Warning("Processing completed. Missing files detected");
        foreach (var file in missingFiles)
            logger.Warning("File {File} was not found in the supplied archive(s)", Path.Combine(file.Location, file.Name));
    }
}
