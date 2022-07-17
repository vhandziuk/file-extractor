namespace FileExtractor.Utils;

public interface IEnvironment
{
    string GetFolderPath(Environment.SpecialFolder folder);
}