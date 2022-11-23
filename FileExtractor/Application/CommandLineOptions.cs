using CommandLine;

namespace FileExtractor.Application;

internal sealed class CommandLineOptions : ICommandLineOptions
{
#pragma warning disable 8618
    [Option('c', "configuration", Required = false, HelpText = "Path to a CSV configuration file")]
    public string Configuration { get; set; }
    [Option('s', "source", Required = false, HelpText = "Path to the source directory")]
    public string Source { get; set; }
    [Option('d', "destination", Required = false, HelpText = "Path to the destination directory")]
    public string Destination { get; set; }
    [Option("cache-configuration", Required = false, HelpText = "Save current configuration as default")]
    public bool CacheConfiguration { get; set; }
    [Option("no-wait", Required = false, HelpText = "Do not wait for a key press to exit the app")]
    public bool NoWait { get; set; }
#pragma warning restore 8618
}