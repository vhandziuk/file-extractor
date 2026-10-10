using FileExtractor.Application.Options;

namespace FileExtractor.Application.Commands;

/// <summary>
/// Prints the files in the archives named on the command line.
/// </summary>
internal interface IListCommand
{
    Task RunAsync(IListOptions options, CancellationToken cancellationToken);
}
