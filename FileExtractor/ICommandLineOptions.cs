namespace FileExtractor;

internal interface ICommandLineOptions
{
    string Configuration { get; set; }
    string Source { get; set; }
    string Destination { get; set; }
}
