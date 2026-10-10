using CommandLine;

namespace FileExtractor.Application.Options;

// FollowSymbolicLinks is declared by each verb, because its help text names the verb's own target directory option
internal abstract class ExtractionOptionsBase : CommonOptionsBase, IExtractionOptions
{
    [Option('p', "max-parallelism", Required = false, HelpText = "Maximum number of archives extracted in parallel (default: number of processors)")]
    public int? MaxParallelism { get; set; }

    public abstract bool FollowSymbolicLinks { get; set; }
}
