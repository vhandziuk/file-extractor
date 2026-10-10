namespace FileExtractor.Utils;

/// <summary>
/// Standard output for command results, such as an archive listing, as opposed to log messages.
/// </summary>
public interface IConsole
{
    void WriteLine(string value);
}
