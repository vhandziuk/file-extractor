using FileExtractor.Application.Options;

namespace FileExtractor.Application.Commands;

/// <summary>
/// Extracts the files listed in a CSV configuration from the archives in a source folder.
/// </summary>
internal interface IConfigurationCommand
{
    Task RunAsync(IConfigurationOptions options, CancellationToken cancellationToken);
}
