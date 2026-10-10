using CommandLine;
using CommandLine.Text;

namespace FileExtractor.Application.Options;

// The default verb lets "fileextractor <archive>" work like unzip. An archive named exactly like a verb
// or its alias ("list", "c") still needs the explicit "e".
[Verb("extract", isDefault: true, aliases: ["e"], HelpText = "Extract files from archives named on the command line")]
internal sealed class ExtractOptions : ExtractionOptionsBase, IExtractOptions
{
    [Value(0, MetaName = "archive", Required = true, HelpText = @"Archive file, folder with archives, or wildcard path such as C:\Archives\*.zip")]
    public string Archive { get; set; } = string.Empty;
    [Value(1, MetaName = "files", Required = false, HelpText = "Files to extract: names or paths in the archive, with * and ? wildcards in the name (default: all files)")]
    public IEnumerable<string> Files { get; set; } = [];
    [Option('o', "output", Required = false, HelpText = "Path to the output directory, created when missing (default: current directory)")]
    public string? Output { get; set; }
    // a single value: a list option would also take the archive and files that follow it
    [Option('x', "exclude", Required = false, HelpText = "Comma-separated files not to extract, in the same format as the files to extract, for example *.tmp,*.log")]
    public string? Exclude { get; set; }
    [Option("flat", Required = false, HelpText = "Extract all files directly into the output directory, without the folders of the archive")]
    public bool Flat { get; set; }
    [Option("archive-folder", Required = false, HelpText = "Extract each archive into a subdirectory of the output directory named after the archive")]
    public bool ArchiveFolder { get; set; }
    [Option('r', "recursive", Required = false, HelpText = "Search for archives in subdirectories as well when the archive is a folder or wildcard path")]
    public bool Recursive { get; set; }
    [Option("follow-symlinks", Required = false, HelpText = "Extract files into folders below the output directory that are symbolic links or junctions")]
    public override bool FollowSymbolicLinks { get; set; }

    [Usage(ApplicationAlias = "fileextractor")]
    public static IEnumerable<Example> Examples =>
    [
        new("Extract all files of an archive into the current directory, keeping its folders",
            new ExtractOptions { Archive = @"C:\Downloads\package.zip" }),
        new("Extract the text files of the docs folder and all DLLs from every 7z archive in a folder",
            new ExtractOptions { Archive = @"C:\Archives\*.7z", Files = ["docs/*.txt", "*.dll"], Output = @"C:\Output" }),
        new("Extract all files except temporary ones into one subdirectory per archive, without the archive folders",
            new ExtractOptions { Archive = @"C:\Archives", Output = @"C:\Output", ArchiveFolder = true, Flat = true, Exclude = "*.tmp,*.log" })
    ];
}
