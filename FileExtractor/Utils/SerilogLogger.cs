using Serilog;

namespace FileExtractor.Utils;

internal sealed class SerilogLogger<T> : ILogger<T>
{
    private readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => SerilogLoggerFactory.Create<T>());
    private ILogger Logger => _logger.Value;

    public void Information(string messageTemplate) => Logger.Information(messageTemplate);
    public void Information(string messageTemplate, params object[] propertyValues) => Logger.Information(messageTemplate, propertyValues);

    public void Warning(string messageTemplate) => Logger.Warning(messageTemplate);
    public void Warning(string messageTemplate, params object[] propertyValues) => Logger.Warning(messageTemplate, propertyValues);

    public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => Logger.Error(exception, messageTemplate, propertyValues);
}