using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

using SpecialFolder = System.Environment.SpecialFolder;

namespace FileExtractor.Common.Logging;

internal static class SerilogLoggerFactory
{
    private const string EventTimingAndLevel = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}]";
    private const string ConsoleOutputTemplate = $"{EventTimingAndLevel} {{Message:lj}}{{NewLine}}";
    private const string FileOutputTemplate = $"{EventTimingAndLevel} [{{ProcessId}}] [{{ThreadId}}] [{{SourceContext}}] {{Message:lj}}{{NewLine}}{{Exception}}";

    private static readonly string _logFilePath = Path.Combine(
        Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "File Extractor", "Logs", "Application.log");

    private static readonly Lazy<Serilog.ILogger> _logger =
        new Lazy<Serilog.ILogger>(() =>
            new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: ConsoleOutputTemplate,
                    theme: AnsiConsoleTheme.Literate)
                .WriteTo.File(
                    path: _logFilePath,
                    outputTemplate: FileOutputTemplate,
                    fileSizeLimitBytes: 100_000L,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 5,
                    encoding: Encoding.Unicode
                )
                .CreateLogger());

    public static Serilog.ILogger Create<T>() =>
        _logger.Value.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
}