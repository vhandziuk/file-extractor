namespace FileExtractor.Application;

internal interface ICommandLineOptions
{
    string? Configuration { get; }
    string? Source { get; }
    string? Destination { get; }
    bool Recursive { get; }
    bool CacheConfiguration { get; }
    bool NoWait { get; }
    int? MaxParallelism { get; }
    bool FollowSymbolicLinks { get; }
}
