namespace FileExtractor;

internal interface IApp
{
    Task RunAsync(string sourcePath, string destinationPath, string configurationPath);
}