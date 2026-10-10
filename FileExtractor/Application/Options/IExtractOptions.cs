namespace FileExtractor.Application.Options;

internal interface IExtractOptions : IExtractionOptions
{
    string Archive { get; }
    IEnumerable<string> Files { get; }
    string? Output { get; }
    IEnumerable<string> Exclude { get; }
    bool Flat { get; }
    bool ArchiveFolder { get; }
    bool Recursive { get; }
}
