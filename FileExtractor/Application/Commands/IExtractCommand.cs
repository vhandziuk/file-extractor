using FileExtractor.Application.Options;

namespace FileExtractor.Application.Commands;

/// <summary>
/// Extracts the files named on the command line from the archives named on the command line.
/// </summary>
internal interface IExtractCommand
{
    Task RunAsync(IExtractOptions options, CancellationToken cancellationToken);
}
