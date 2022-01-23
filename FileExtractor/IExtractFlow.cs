namespace FileExtractor;

internal interface IExtractFlow
{
    ValueTask Extract(string sourcePath, string destinationPath, string configurationPath);
}