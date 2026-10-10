using FileExtractor.Application.Options;

namespace FileExtractor.Application;

internal interface IApp
{
    /// <summary>
    /// Runs the command of the parsed verb <paramref name="options"/>; failures and cancellation are logged, not thrown.
    /// </summary>
    ValueTask RunAsync(ICommonOptions options, CancellationToken cancellationToken);
}
