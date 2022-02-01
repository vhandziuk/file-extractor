using Serilog;

namespace FileExtractor.Utils;

internal sealed class Logger<T> : ILogger<T>
{
    private readonly ILogger _logger = LoggerFactory.GetLogger<T>();

    public void Information(string messageTemplate) => _logger.Information(messageTemplate);
    public void Information(string messageTemplate, params object[] propertyValues) => _logger.Information(messageTemplate, propertyValues);

    public void Warning(string messageTemplate) => _logger.Warning(messageTemplate);
    public void Warning(string messageTemplate, params object[] propertyValues) => _logger.Warning(messageTemplate, propertyValues);

    public void Error(string messageTemplate) => _logger.Error(messageTemplate);
}
