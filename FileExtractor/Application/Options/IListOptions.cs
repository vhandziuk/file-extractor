namespace FileExtractor.Application.Options;

internal interface IListOptions : ICommonOptions
{
    string Archive { get; }
    bool Recursive { get; }
}
