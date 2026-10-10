using CommandLine;

namespace FileExtractor.Application.Options;

// CommandLineParser reads the options of base classes as well, so each verb declares the shared ones only once
internal abstract class CommonOptionsBase : ICommonOptions
{
    [Option("no-wait", Required = false, HelpText = "Do not wait for a key press to exit the app")]
    public bool NoWait { get; set; }
}
