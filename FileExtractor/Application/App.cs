using FileExtractor.Application.Commands;
using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;

namespace FileExtractor.Application;

internal sealed class App(
    IConfigurationCommand configurationCommand,
    IExtractCommand extractCommand,
    ILogger<App> logger) : IApp
{
    public async ValueTask RunAsync(ICommonOptions options, CancellationToken cancellationToken)
    {
        try
        {
            await RunCommandAsync(options, cancellationToken);
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

    private Task RunCommandAsync(ICommonOptions options, CancellationToken cancellationToken) =>
        options switch
        {
            IConfigurationOptions configurationOptions => configurationCommand.RunAsync(configurationOptions, cancellationToken),
            IExtractOptions extractOptions => extractCommand.RunAsync(extractOptions, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.GetType().Name, "Unsupported command line verb")
        };
}
