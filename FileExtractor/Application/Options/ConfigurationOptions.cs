using CommandLine;
using CommandLine.Text;

namespace FileExtractor.Application.Options;

[Verb("config", HelpText = "Extract the files listed in a CSV configuration from the archives in a folder")]
internal sealed class ConfigurationOptions : ExtractionOptionsBase, IConfigurationOptions
{
    [Value(0, MetaName = "source", Required = true, HelpText = "Path to the source directory with the archives")]
    public string Source { get; set; } = string.Empty;
    [Option('c', "configuration", Required = false, HelpText = "Path to a CSV configuration file (default: configuration.csv in the source directory, then the cached configuration)")]
    public string? Configuration { get; set; }
    [Option('o', "output", Required = false, HelpText = "Path to the output directory, created when missing (default: the source directory)")]
    public string? Output { get; set; }
    [Option('r', "recursive", Required = false, HelpText = "Search for archives in subdirectories of the source directory as well")]
    public bool Recursive { get; set; }
    [Option("cache-configuration", Required = false, HelpText = "Save current configuration as default")]
    public bool CacheConfiguration { get; set; }
    [Option("follow-symlinks", Required = false, HelpText = "Extract files into folders below the output directory that are symbolic links or junctions")]
    public override bool FollowSymbolicLinks { get; set; }

    [Usage(ApplicationAlias = "fileextractor")]
    public static IEnumerable<Example> Examples =>
    [
        new("Extract the files listed in C:\\Archives\\configuration.csv from the archives in C:\\Archives into the same folder",
            new ConfigurationOptions { Source = @"C:\Archives" }),
        new("Extract the files listed in a configuration file from the archives in a folder and its subfolders into another folder",
            new ConfigurationOptions { Source = @"C:\Archives", Output = @"C:\Output", Configuration = @"C:\Config\files.csv", Recursive = true })
    ];
}
