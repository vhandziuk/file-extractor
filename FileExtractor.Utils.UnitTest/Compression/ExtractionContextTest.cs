using System.Linq;
using System.Threading.Tasks;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression;

public class ExtractionContextTest
{
    private const string SomeOutputPath = @"C:\Output";
    private const string SomeFilePath = @"C:\Output\SomeName.dat";

    private static readonly FileInfoData[] SomeFiles = [new("", "SomeName.dat", "SomeDirectory")];

    private readonly ExtractionContext _sut = new(new ExtractionSettings(SomeOutputPath, SomeFiles, 1));

    [Fact]
    public void Constructor_ValuesProvided_ExposesOutputPathAndFiles()
    {
        Assert.Equal(SomeOutputPath, _sut.OutputPath);
        Assert.Same(SomeFiles, _sut.Files);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_FollowSymbolicLinksProvided_ExposesIt(bool followSymbolicLinks)
    {
        var context = new ExtractionContext(new ExtractionSettings(SomeOutputPath, SomeFiles, 1) { FollowSymbolicLinks = followSymbolicLinks });

        Assert.Equal(followSymbolicLinks, context.FollowSymbolicLinks);
    }

    [Fact]
    public void TryClaimFilePath_PathIsNotClaimed_ReturnsTrue()
    {
        var result = _sut.TryClaimFilePath(SomeFilePath);

        Assert.True(result);
    }

    [Theory]
    [InlineData(SomeFilePath)]
    [InlineData(@"c:\OUTPUT\somename.DAT")]
    public void TryClaimFilePath_PathIsAlreadyClaimed_ReturnsFalse(string path)
    {
        _sut.TryClaimFilePath(SomeFilePath);

        var result = _sut.TryClaimFilePath(path);

        Assert.False(result);
    }

    [Fact]
    public void TryClaimFilePath_OtherPathIsClaimed_ReturnsTrue()
    {
        _sut.TryClaimFilePath(SomeFilePath);

        var result = _sut.TryClaimFilePath(@"C:\Output\SomeOtherName.dat");

        Assert.True(result);
    }

    [Fact]
    public void TryClaimFilePath_SamePathClaimedConcurrently_GrantsExactlyOneClaim()
    {
        var results = new bool[100];

        Parallel.For(0, results.Length, index => results[index] = _sut.TryClaimFilePath(SomeFilePath));

        Assert.Equal(1, results.Count(result => result));
    }
}
