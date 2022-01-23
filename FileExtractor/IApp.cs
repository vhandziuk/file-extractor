namespace FileExtractor;

internal interface IApp
{
    ValueTask Run(string sourcePath, string destinationPath, string configurationPath);
}