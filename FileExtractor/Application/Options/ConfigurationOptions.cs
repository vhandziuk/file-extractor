using CommandLine;

namespace FileExtractor.Application.Options;

// The default verb keeps "fileextractor -s <folder>", used by the folder context menu and existing scripts, working.
[Verb("config", isDefault: true, HelpText = "Extract the files listed in a CSV configuration from the archives in a folder")]
internal sealed class ConfigurationOptions : ExtractionOptionsBase, IConfigurationOptions
{
    [Option('c', "configuration", Required = false, HelpText = "Path to a CSV configuration file")]
    public string? Configuration { get; set; }
    [Option('s', "source", Required = false, HelpText = "Path to the source directory")]
    public string? Source { get; set; }
    [Option('d', "destination", Required = false, HelpText = "Path to the destination directory")]
    public string? Destination { get; set; }
    [Option('r', "recursive", Required = false, HelpText = "Search for archives in subdirectories of the source directory as well")]
    public bool Recursive { get; set; }
    [Option("cache-configuration", Required = false, HelpText = "Save current configuration as default")]
    public bool CacheConfiguration { get; set; }
}
