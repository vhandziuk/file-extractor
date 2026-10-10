namespace FileExtractor.Application.Options;

/// <summary>
/// Options shared by the verbs that extract files.
/// </summary>
internal interface IExtractionOptions : ICommonOptions
{
    int? MaxParallelism { get; }
    bool FollowSymbolicLinks { get; }
}
