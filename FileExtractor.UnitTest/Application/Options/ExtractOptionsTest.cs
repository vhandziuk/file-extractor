using CommandLine;
using FileExtractor.Application.Options;
using Xunit;

namespace FileExtractor.UnitTest.Application.Options;

public class ExtractOptionsTest
{
    private readonly Parser _parser = new(settings => settings.HelpWriter = null);

    [Fact]
    public void ParseArguments_NoVerbGiven_ParsesExtractOptions()
    {
        var result = ParseArguments(@"C:\Source\SomeArchive.zip", "*.txt", "-o", @"C:\Output");

        var options = Assert.IsType<ExtractOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal(@"C:\Source\SomeArchive.zip", options.Archive);
        Assert.Equal(["*.txt"], options.Files);
        Assert.Equal(@"C:\Output", options.Output);
    }

    [Theory]
    [InlineData("extract")]
    [InlineData("x")]
    public void ParseArguments_VerbOrAliasGiven_ParsesExtractOptions(string verb)
    {
        var result = ParseArguments(verb, @"C:\Source\SomeArchive.zip", "--flat", "--archive-folder", "-x", "*.tmp,*.log");

        var options = Assert.IsType<ExtractOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.True(options.Flat);
        Assert.True(options.ArchiveFolder);
        Assert.Equal("*.tmp,*.log", options.Exclude);
    }

    [Fact]
    public void ParseArguments_ExcludeGivenBeforeArchiveAndFiles_TakesOnlyItsOwnValue()
    {
        var result = ParseArguments("-x", "*.tmp,*.log", @"C:\Source\SomeArchive.zip", "*.txt");

        var options = Assert.IsType<ExtractOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal("*.tmp,*.log", options.Exclude);
        Assert.Equal(@"C:\Source\SomeArchive.zip", options.Archive);
        Assert.Equal(["*.txt"], options.Files);
    }

    [Theory]
    [InlineData("-j")]
    [InlineData("--jobs")]
    public void ParseArguments_JobsGiven_ParsesMaxParallelism(string option)
    {
        var result = ParseArguments(@"C:\Source\SomeArchive.zip", option, "4");

        var options = Assert.IsType<ExtractOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal(4, options.MaxParallelism);
    }

    [Fact]
    public void ParseArguments_ArchiveIsMissing_Fails()
    {
        var result = ParseArguments("-o", @"C:\Output");

        Assert.IsType<NotParsed<object>>(result);
    }

    [Fact]
    public void Examples_FormattedAsHelpShowsThem_ParseBackToSameOptions()
    {
        foreach (var example in ExtractOptions.Examples)
        {
            var commandLine = _parser.FormatCommandLine((ExtractOptions)example.Sample);

            var result = ParseArguments(commandLine.Split(' '));

            var options = Assert.IsType<ExtractOptions>(Assert.IsType<Parsed<object>>(result).Value);
            Assert.Equal(commandLine, _parser.FormatCommandLine(options));
        }
    }

    private ParserResult<object> ParseArguments(params string[] args) =>
        _parser.ParseArguments(args, typeof(ConfigurationOptions), typeof(ExtractOptions), typeof(ListOptions));
}
