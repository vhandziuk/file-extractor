using Serilog;

namespace FileExtractor.Utils;

internal static class LoggerFactory
{
    private static readonly object _lock = new();
    private static readonly Dictionary<Type, ILogger> _loggers = new();

    public static ILogger GetLogger<T>()
    {
        lock (_lock)
        {
            if (_loggers.TryGetValue(typeof(T), out var existingLogger))
            {
                return existingLogger;
            }

            var newLogger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();
            _loggers[typeof(T)] = newLogger;

            return newLogger;
        }
    }
}