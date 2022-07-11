namespace FileExtractor.Application;

internal interface ICommandLineOptions
{
    string Configuration { get; }
    string Source { get; }
    string Destination { get; }
    bool CacheConfiguration { get; }
}