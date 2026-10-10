namespace FileExtractor.Application.Options;

internal interface IConfigurationOptions : IExtractionOptions
{
    string Source { get; }
    string? Configuration { get; }
    string? Output { get; }
    bool Recursive { get; }
    bool CacheConfiguration { get; }
}
