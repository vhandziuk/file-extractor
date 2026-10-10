namespace FileExtractor.Application.Options;

internal interface IExtractOptions : IExtractionOptions
{
    string Archive { get; }
    IEnumerable<string> Files { get; }
    string? Output { get; }
    /// <summary>
    /// Comma-separated patterns of the files not to extract, or <see langword="null"/> for none.
    /// </summary>
    string? Exclude { get; }
    bool Flat { get; }
    bool ArchiveFolder { get; }
    bool Recursive { get; }
}
