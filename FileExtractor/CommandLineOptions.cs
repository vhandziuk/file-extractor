using CommandLine;

namespace FileExtractor;

internal sealed class CommandLineOptions
{
#pragma warning disable 8618
    [Option('c', "configuration", Required = false, HelpText = "Path to a CSV configuration file")]
    public string Configuration { get; set; }
    [Option('o', "output", Required = false, HelpText = "Path to the output directory")]
    public string Output { get; set; }
#pragma warning restore 8618
}