namespace FileExtractor.Application.Options;

/// <summary>
/// Options shared by all verbs.
/// </summary>
internal interface ICommonOptions
{
    bool NoWait { get; }
}
