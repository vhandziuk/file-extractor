namespace FileExtractor.Application.Options;

internal interface IConfigurationOptions : IExtractionOptions
{
    string? Configuration { get; }
    string? Source { get; }
    string? Destination { get; }
    bool Recursive { get; }
    bool CacheConfiguration { get; }
}
