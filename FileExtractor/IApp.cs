namespace FileExtractor;

internal interface IApp
{
    Task Run(string sourcePath, string destinationPath, string configurationPath);
}