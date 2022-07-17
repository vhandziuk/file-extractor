namespace FileExtractor.Utils;

public sealed class EnvironmentWrapper : IEnvironment
{
    public string GetFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
}