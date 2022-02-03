using Serilog;

namespace FileExtractor.Utils;

internal sealed class SerilogLogger<T> : ILogger<T>
{
    private static readonly string _outputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{ProcessId}] [{ThreadId}] [{ClassName}] {Message:lj}{NewLine}{Exception}";

    private readonly Lazy<ILogger> _logger =
        new Lazy<ILogger>(() => new LoggerConfiguration()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ClassName", typeof(T).Name)
            .WriteTo.ColoredConsole(outputTemplate: _outputTemplate)
            .CreateLogger());

    private ILogger Logger => _logger.Value;

    public void Information(string messageTemplate) => Logger.Information(messageTemplate);
    public void Information(string messageTemplate, params object[] propertyValues) => Logger.Information(messageTemplate, propertyValues);

    public void Warning(string messageTemplate) => Logger.Warning(messageTemplate);
    public void Warning(string messageTemplate, params object[] propertyValues) => Logger.Warning(messageTemplate, propertyValues);

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => Logger.Error(exception, messageTemplate, propertyValues);
}
