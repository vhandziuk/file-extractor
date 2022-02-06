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
#pragma warning restore 8618
}