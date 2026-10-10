using CommandLine;

namespace FileExtractor.Application.Options;

// The default verb lets "fileextractor <archive>" work like unzip. An archive named exactly like a verb
// ("list", "config") still needs the explicit "x".
[Verb("extract", isDefault: true, aliases: ["x"], HelpText = "Extract files from archives named on the command line")]
internal sealed class ExtractOptions : ExtractionOptionsBase, IExtractOptions
{
    [Value(0, MetaName = "archive", Required = true, HelpText = @"Archive file, folder with archives, or wildcard path such as C:\Archives\*.zip")]
    public string Archive { get; set; } = string.Empty;
    [Value(1, MetaName = "files", Required = false, HelpText = "Files to extract: names or paths in the archive, with * and ? wildcards in the name (default: all files)")]
    public IEnumerable<string> Files { get; set; } = [];
    [Option('o', "output", Required = false, HelpText = "Path to the output directory, created when missing (default: current directory)")]
    public string? Output { get; set; }
    [Option('x', "exclude", Required = false, HelpText = "Files not to extract, in the same format as the files to extract")]
    public IEnumerable<string> Exclude { get; set; } = [];
    [Option("flat", Required = false, HelpText = "Extract all files directly into the output directory, without the folders of the archive")]
    public bool Flat { get; set; }
    [Option("archive-folder", Required = false, HelpText = "Extract each archive into a subdirectory of the output directory named after the archive")]
    public bool ArchiveFolder { get; set; }
    [Option('r', "recursive", Required = false, HelpText = "Search for archives in subdirectories as well when the archive is a folder or wildcard path")]
    public bool Recursive { get; set; }
}
