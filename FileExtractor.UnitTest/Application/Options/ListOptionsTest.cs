using CommandLine;
using FileExtractor.Application.Options;
using Xunit;

namespace FileExtractor.UnitTest.Application.Options;

public class ListOptionsTest
{
    private readonly Parser _parser = new(settings => settings.HelpWriter = null);

    [Theory]
    [InlineData("list")]
    [InlineData("l")]
    public void ParseArguments_VerbOrAliasGiven_ParsesListOptions(string verb)
    {
        var result = ParseArguments(verb, @"C:\Archives\*.zip", "-r");

        var options = Assert.IsType<ListOptions>(Assert.IsType<Parsed<object>>(result).Value);
        Assert.Equal(@"C:\Archives\*.zip", options.Archive);
        Assert.True(options.Recursive);
    }

    [Fact]
    public void Examples_FormattedAsHelpShowsThem_ParseBackToSameOptions()
    {
        foreach (var example in ListOptions.Examples)
        {
            var commandLine = _parser.FormatCommandLine((ListOptions)example.Sample);

            var result = ParseArguments(commandLine.Split(' '));

            var options = Assert.IsType<ListOptions>(Assert.IsType<Parsed<object>>(result).Value);
            Assert.Equal(commandLine, _parser.FormatCommandLine(options));
        }
    }

    private ParserResult<object> ParseArguments(params string[] args) =>
        _parser.ParseArguments(args, typeof(ConfigurationOptions), typeof(ExtractOptions), typeof(ListOptions));
}
