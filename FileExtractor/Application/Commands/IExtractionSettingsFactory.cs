using FileExtractor.Application.Options;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;

namespace FileExtractor.Application.Commands;

internal interface IExtractionSettingsFactory
{
    /// <summary>
    /// Creates the settings of an extraction into <paramref name="outputPath"/> from the options shared by all
    /// extracting verbs. A missing or non-positive maximum parallelism falls back to the number of processors.
    /// </summary>
    ExtractionSettings Create(IExtractionOptions options, string outputPath, IReadOnlyList<FileInfoData> files);
}
