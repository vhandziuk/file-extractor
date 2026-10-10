using CommandLine;

namespace FileExtractor.Application.Options;

[Verb("list", aliases: ["l"], HelpText = "List the files in archives without extracting them")]
internal sealed class ListOptions : CommonOptionsBase, IListOptions
{
    [Value(0, MetaName = "archive", Required = true, HelpText = @"Archive file, folder with archives, or wildcard path such as C:\Archives\*.zip")]
    public string Archive { get; set; } = string.Empty;
    [Option('r', "recursive", Required = false, HelpText = "Search for archives in subdirectories as well when the archive is a folder or wildcard path")]
    public bool Recursive { get; set; }
}
