using CommandLine;
using FileExtractor.Application.Options;
using Xunit;

namespace FileExtractor.UnitTest.Application.Options;

public class ConfigurationOptionsTest
{
    private readonly Parser _parser = new(settings => settings.HelpWriter = null);

    [Theory]
    [InlineData("config")]
    [InlineData("c")]
    public void ParseArguments_VerbOrAliasGiven_ParsesConfigurationOptions(string verb)
    {
        var result = ParseArguments(verb, @"C:\Source", "-c", @"C:\Config\custom.csv", "-o", @"C:\Output", "-r");

        var options = Assert.IsType<ConfigurationOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal(@"C:\Source", options.Source);
        Assert.Equal(@"C:\Config\custom.csv", options.Configuration);
        Assert.Equal(@"C:\Output", options.Output);
        Assert.True(options.Recursive);
    }

    [Fact]
    public void ParseArguments_JobsGiven_ParsesMaxParallelism()
    {
        var result = ParseArguments("config", @"C:\Source", "-j", "2");

        var options = Assert.IsType<ConfigurationOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal(2, options.MaxParallelism);
    }

    [Fact]
    public void ParseArguments_SourceIsMissing_Fails()
    {
        var result = ParseArguments("config", "-c", @"C:\Config\custom.csv");

        Assert.IsType<NotParsed<object>>(result);
    }

    [Theory]
    [InlineData("config", "-s", @"C:\Source")]
    [InlineData("config", @"C:\Source", "-d", @"C:\Output")]
    [InlineData("-s", @"C:\Source")]
    public void ParseArguments_OptionsOfEarlierVersionsGiven_Fails(params string[] args)
    {
        var result = ParseArguments(args);

        Assert.IsType<NotParsed<object>>(result);
    }

    [Fact]
    public void Examples_FormattedAsHelpShowsThem_ParseBackToSameOptions()
    {
        foreach (var example in ConfigurationOptions.Examples)
        {
            var commandLine = _parser.FormatCommandLine((ConfigurationOptions)example.Sample);

            var result = ParseArguments(commandLine.Split(' '));

            var options = Assert.IsType<ConfigurationOptions>(Assert.IsType<Parsed<object>>(result).Value);
            Assert.Equal(commandLine, _parser.FormatCommandLine(options));
        }
    }

    private ParserResult<object> ParseArguments(params string[] args) =>
        _parser.ParseArguments(args, typeof(ConfigurationOptions), typeof(ExtractOptions), typeof(ListOptions));
}
