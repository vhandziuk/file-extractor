namespace FileExtractor.Utils;

public interface IEnvironment
{
    int ProcessorCount { get; }

    string GetFolderPath(Environment.SpecialFolder folder);
}
