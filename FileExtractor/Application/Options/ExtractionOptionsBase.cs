using CommandLine;

namespace FileExtractor.Application.Options;

internal abstract class ExtractionOptionsBase : CommonOptionsBase, IExtractionOptions
{
    [Option('p', "max-parallelism", Required = false, HelpText = "Maximum number of archives extracted in parallel (default: number of processors)")]
    public int? MaxParallelism { get; set; }
    [Option("follow-symlinks", Required = false, HelpText = "Extract files into folders below the destination that are symbolic links or junctions")]
    public bool FollowSymbolicLinks { get; set; }
}
