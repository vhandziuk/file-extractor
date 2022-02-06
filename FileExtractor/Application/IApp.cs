namespace FileExtractor.Application;

internal interface IApp
{
    ValueTask RunAsync(ICommandLineOptions options);
}