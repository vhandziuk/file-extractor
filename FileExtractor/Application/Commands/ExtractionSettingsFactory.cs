using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;

namespace FileExtractor.Application.Commands;

internal sealed class ExtractionSettingsFactory(
    IEnvironment environment,
    ILogger<ExtractionSettingsFactory> logger) : IExtractionSettingsFactory
{
    public ExtractionSettings Create(IExtractionOptions options, string outputPath, IReadOnlyList<FileInfoData> files)
    {
        if (options.FollowSymbolicLinks)
            logger.Warning("Symbolic links and junctions below the destination path are followed");

        return new ExtractionSettings(outputPath, files, GetMaxDegreeOfParallelism(options.MaxParallelism))
        {
            FollowSymbolicLinks = options.FollowSymbolicLinks
        };
    }

    private int GetMaxDegreeOfParallelism(int? maxParallelism)
    {
        if (maxParallelism is { } value and > 0)
            return value;

        if (maxParallelism is not null)
            logger.Warning("Maximum parallelism must be a positive number. Using {ProcessorCount} instead", environment.ProcessorCount);

        return environment.ProcessorCount;
    }
}
