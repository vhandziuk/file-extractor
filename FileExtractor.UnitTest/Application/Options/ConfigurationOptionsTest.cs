using CommandLine;
using FileExtractor.Application.Options;
using Xunit;

namespace FileExtractor.UnitTest.Application.Options;

public class ConfigurationOptionsTest
{
    private readonly Parser _parser = new(settings => settings.HelpWriter = null);

    [Fact]
    public void ParseArguments_ConfigVerbGiven_ParsesConfigurationOptions()
    {
        var result = ParseArguments("config", "-s", @"C:\Source", "-c", @"C:\Config\custom.csv", "-r");

        var options = Assert.IsType<ConfigurationOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal(@"C:\Source", options.Source);
        Assert.Equal(@"C:\Config\custom.csv", options.Configuration);
        Assert.True(options.Recursive);
    }

    [Fact]
    public void ParseArguments_SourceGivenWithoutVerb_Fails()
    {
        var result = ParseArguments("-s", @"C:\Source");

        Assert.IsType<NotParsed<object>>(result);
    }

    private ParserResult<object> ParseArguments(params string[] args) =>
        _parser.ParseArguments(args, typeof(ConfigurationOptions), typeof(ExtractOptions), typeof(ListOptions));
}
