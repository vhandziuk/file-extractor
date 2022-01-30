namespace FileExtractor;

internal interface IApp
{
    ValueTask RunAsync(ICommandLineOptions options);
}