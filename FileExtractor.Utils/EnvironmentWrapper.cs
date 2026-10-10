namespace FileExtractor.Utils;

public sealed class EnvironmentWrapper : IEnvironment
{
    public int ProcessorCount => Environment.ProcessorCount;

    public string GetFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
}
