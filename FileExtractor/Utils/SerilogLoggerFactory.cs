using System.Text;
using Serilog;
using Serilog.Core;

using SpecialFolder = System.Environment.SpecialFolder;

namespace FileExtractor.Utils;

internal static class SerilogLoggerFactory
{
    private const string EventTimingAndLevel = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}]";
    private const string EventDetails = "{Message:lj}{NewLine}{Exception}";

    private const string ConsoleOutputTemplate = $"{EventTimingAndLevel} {EventDetails}";
    private const string FileOutputTemplate = $"{EventTimingAndLevel} [{{ProcessId}}] [{{ThreadId}}] [{{SourceContext}}] {EventDetails}";

    private static readonly string _logFilePath = Path.Combine(
        Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "FileExtractor", "Application.log");

    private static readonly Lazy<ILogger> _logger =
        new Lazy<ILogger>(() =>
            new LoggerConfiguration()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.ColoredConsole(
                    outputTemplate: ConsoleOutputTemplate)
                .WriteTo.File(
                    path: _logFilePath,
                    outputTemplate: FileOutputTemplate,
                    fileSizeLimitBytes: 100_000L,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 5,
                    encoding: Encoding.Unicode
                )
                .CreateLogger());

    public static ILogger Create<T>() =>
        _logger.Value.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);
}