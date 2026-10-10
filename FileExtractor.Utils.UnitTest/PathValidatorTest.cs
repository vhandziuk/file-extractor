using Xunit;

namespace FileExtractor.Utils.UnitTest;

public class PathValidatorTest
{
    private const string SomeRootPath = @"C:\Output";

    private readonly PathValidator _sut = new();

    [Theory]
    [InlineData(@"C:\Output")]
    [InlineData(@"C:\Output\")]
    [InlineData(@"C:\Output\SomeName.dat")]
    [InlineData(@"C:\Output\Test\Nested\SomeName.dat")]
    [InlineData(@"c:\output\test\SomeName.dat")]
    [InlineData(@"C:\Output\Test\..\SomeName.dat")]
    public void IsContained_PathIsRootOrBelowIt_ReturnsTrue(string path)
    {
        var result = _sut.IsContained(SomeRootPath, path);

        Assert.True(result);
    }

    [Theory]
    [InlineData(@"C:\Output\..\SomeName.dat")]
    [InlineData(@"C:\Output\Test\..\..\Windows\SomeName.dll")]
    [InlineData(@"C:\Windows\SomeName.dll")]
    [InlineData(@"C:\Output2\SomeName.dat")]
    [InlineData(@"C:\")]
    [InlineData(@"\Windows\SomeName.dll")]
    [InlineData(@"D:\Output\SomeName.dat")]
    [InlineData(@"\\server\share\SomeName.dat")]
    [InlineData(@"C:\Output\SomeName.dat:stream")]
    public void IsContained_PathIsOutsideRootOrAddressesStream_ReturnsFalse(string path)
    {
        var result = _sut.IsContained(SomeRootPath, path);

        Assert.False(result);
    }

    [Theory]
    [InlineData(@"C:\", @"C:\SomeName.dat")]
    [InlineData(@"\\server\share", @"\\server\share\SomeName.dat")]
    public void IsContained_RootIsVolumeOrShare_ReturnsTrueForPathBelowIt(string rootPath, string path)
    {
        var result = _sut.IsContained(rootPath, path);

        Assert.True(result);
    }

    [Theory]
    [InlineData("SomeName.dat")]
    [InlineData("Some Name.dat")]
    [InlineData(".hidden")]
    [InlineData("CONSOLE.txt")]
    [InlineData("COM10")]
    public void IsValidFileName_NameIsPlainFileName_ReturnsTrue(string fileName)
    {
        var result = _sut.IsValidFileName(fileName);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData(@"..\SomeName.dll")]
    [InlineData("../SomeName.dll")]
    [InlineData(@"Nested\SomeName.dat")]
    [InlineData(@"C:SomeName.dat")]
    [InlineData("SomeName.dat:stream")]
    [InlineData("Some*Name.dat")]
    [InlineData("Some\nName.dat")]
    [InlineData("Some\0Name.dat")]
    [InlineData("SomeName.dat.")]
    [InlineData("SomeName.dat ")]
    [InlineData("CON")]
    [InlineData("nul.txt")]
    [InlineData("Com1.tar.gz")]
    [InlineData("LPT9")]
    [InlineData("AUX .txt")]
    public void IsValidFileName_NameIsUnsafe_ReturnsFalse(string fileName)
    {
        var result = _sut.IsValidFileName(fileName);

        Assert.False(result);
    }
}
