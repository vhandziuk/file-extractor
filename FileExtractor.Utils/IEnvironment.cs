namespace FileExtractor.Utils;

public interface IEnvironment
{
    int ProcessorCount { get; }

    string CurrentDirectory { get; }

    string GetFolderPath(Environment.SpecialFolder folder);
}
