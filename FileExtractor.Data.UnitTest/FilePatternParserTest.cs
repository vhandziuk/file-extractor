using System;
using Xunit;

namespace FileExtractor.Data.UnitTest;

public class FilePatternParserTest
{
    private readonly FilePatternParser _sut = new();

    [Theory]
    [InlineData("SomeName.dat", "SomeName.dat", "")]
    [InlineData("*.dll", "*.dll", "")]
    [InlineData("*", "*", "")]
    [InlineData("docs/*.txt", "*.txt", "docs")]
    [InlineData(@"docs\Nested\Some?.txt", "Some?.txt", @"docs\Nested")]
    [InlineData("docs/Nested/SomeName.dat", "SomeName.dat", @"docs\Nested")]
    [InlineData("/docs/SomeName.dat", "SomeName.dat", "docs")]
    [InlineData("./SomeName.dat", "SomeName.dat", "")]
    [InlineData("./docs//Nested/SomeName.dat", "SomeName.dat", @"docs\Nested")]
    public void Parse_PatternIsValid_ReturnsNameAndLocation(string pattern, string expectedName, string expectedLocation)
    {
        var result = _sut.Parse(pattern);

        Assert.Equal(new FileInfoData("", expectedName, expectedLocation), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("docs/")]
    [InlineData(@"docs\")]
    public void Parse_PatternHasNoFileName_Throws(string pattern)
    {
        var exception = Assert.Throws<ArgumentException>(() => _sut.Parse(pattern));

        Assert.Contains("must end with a file name or wildcard", exception.Message);
    }

    [Theory]
    [InlineData("*/SomeName.dat")]
    [InlineData("doc?/SomeName.dat")]
    [InlineData(@"C:\docs\SomeName.dat")]
    public void Parse_LocationHasWildcardOrDrive_Throws(string pattern)
    {
        var exception = Assert.Throws<ArgumentException>(() => _sut.Parse(pattern));

        Assert.Contains("must be a path inside the archive", exception.Message);
    }
}
