namespace FileExtractor.Utils;

public sealed class ConsoleWrapper : IConsole
{
    public void WriteLine(string value) => Console.WriteLine(value);
}
