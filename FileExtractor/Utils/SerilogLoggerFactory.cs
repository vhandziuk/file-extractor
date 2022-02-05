using Serilog;
using Serilog.Core;

namespace FileExtractor.Utils;

internal static class SerilogLoggerFactory
{
    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{ProcessId}] [{ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    private static readonly Lazy<ILogger> _logger =
        new Lazy<ILogger>(() =>
            new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.ColoredConsole(outputTemplate: OutputTemplate)
                .CreateLogger());

    public static ILogger Create<T>() =>
        _logger.Value.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
}