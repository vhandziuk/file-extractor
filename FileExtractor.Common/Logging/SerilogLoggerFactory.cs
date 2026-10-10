using System.Text;
using FileExtractor.Common.Security;
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

    // Logs are kept per user: in a folder shared by all users, such as ProgramData, another user could plant
    // a junction that redirects the log writes of an elevated run to an arbitrary file.
    private static readonly string _logDirectoryPath = Path.Combine(
        Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "File Extractor", "Logs");

    private static readonly Lazy<Serilog.ILogger> _logger = new(CreateLogger);

    public static Serilog.ILogger Create<T>() =>
        _logger.Value.ForContext(Constants.SourceContextPropertyName, typeof(T).Name);

    private static Serilog.ILogger CreateLogger()
    {
        var configuration = new LoggerConfiguration()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: ConsoleOutputTemplate,
                theme: AnsiConsoleTheme.Literate);

        var isLogDirectorySafe = TryPrepareLogDirectory();
        if (isLogDirectorySafe)
        {
            configuration.WriteTo.File(
                path: Path.Combine(_logDirectoryPath, "Application.log"),
                outputTemplate: FileOutputTemplate,
                fileSizeLimitBytes: 100_000L,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 5,
                encoding: Encoding.Unicode);
        }

        var logger = configuration.CreateLogger();
        if (!isLogDirectorySafe)
            logger.Warning("Log directory {Path} is a symbolic link or junction. Logging to a file is disabled", _logDirectoryPath);

        return logger;
    }

    private static bool TryPrepareLogDirectory()
    {
        // checked before creating the directory, which would otherwise follow the link
        if (IsReparsePoint(_logDirectoryPath) || IsReparsePoint(Path.GetDirectoryName(_logDirectoryPath)!))
            return false;

        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(_logDirectoryPath);
            return true;
        }

        var mediumIntegrityFileSystem = new MediumIntegrityFileSystem();
        mediumIntegrityFileSystem.CreateDirectory(_logDirectoryPath);
        // best-effort: without the label the files keep the default (Medium) integrity of an unlabeled object
        mediumIntegrityFileSystem.ApplyLabel(_logDirectoryPath);

        return true;
    }

    // GetAttributes does not follow links, so a link to a missing target is detected as well
    private static bool IsReparsePoint(string path)
    {
        try
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            return false;
        }
    }
}
