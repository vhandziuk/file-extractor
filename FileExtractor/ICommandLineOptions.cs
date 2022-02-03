namespace FileExtractor;

internal interface ICommandLineOptions
{
    string Configuration { get; }
    string Source { get; }
    string Destination { get; }
}
