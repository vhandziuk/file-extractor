using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest;

public class ReparsePointGuardTest
{
    private const string SomeRootPath = @"C:\Output";
    private const string SomeFilePath = @"C:\Output\Test\Nested\SomeName.dat";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();

    private readonly ReparsePointGuard _sut;

    public ReparsePointGuardTest()
    {
        _sut = new ReparsePointGuard(_fileSystemUtilsMock.Object);
    }

    [Fact]
    public void ContainsReparsePoint_NoPathSegmentIsReparsePoint_ReturnsFalse()
    {
        var result = _sut.ContainsReparsePoint(SomeRootPath, SomeFilePath);

        Assert.False(result);
        _fileSystemUtilsMock.Verify(utils => utils.IsReparsePoint(SomeFilePath), Times.Once);
        _fileSystemUtilsMock.Verify(utils => utils.IsReparsePoint(@"C:\Output\Test\Nested"), Times.Once);
        _fileSystemUtilsMock.Verify(utils => utils.IsReparsePoint(@"C:\Output\Test"), Times.Once);
    }

    [Theory]
    [InlineData(SomeFilePath)]
    [InlineData(@"C:\Output\Test\Nested")]
    [InlineData(@"C:\Output\Test")]
    public void ContainsReparsePoint_PathSegmentBelowRootIsReparsePoint_ReturnsTrue(string reparsePointPath)
    {
        LetBeReparsePoint(reparsePointPath);

        var result = _sut.ContainsReparsePoint(SomeRootPath, SomeFilePath);

        Assert.True(result);
    }

    [Theory]
    [InlineData(@"C:\Output")]
    [InlineData(@"C:\")]
    public void ContainsReparsePoint_OnlyRootOrItsParentIsReparsePoint_ReturnsFalse(string reparsePointPath)
    {
        LetBeReparsePoint(reparsePointPath);

        var result = _sut.ContainsReparsePoint(SomeRootPath, SomeFilePath);

        Assert.False(result);
        _fileSystemUtilsMock.Verify(utils => utils.IsReparsePoint(reparsePointPath), Times.Never);
    }

    [Theory]
    [InlineData(@"C:\Output\")]
    [InlineData(@"c:\output")]
    public void ContainsReparsePoint_RootDiffersInCaseOrTrailingSeparator_ChecksSegmentsBelowRoot(string rootPath)
    {
        LetBeReparsePoint(@"C:\Output\Test");

        var result = _sut.ContainsReparsePoint(rootPath, SomeFilePath);

        Assert.True(result);
    }

    [Fact]
    public void ContainsReparsePoint_PathIsNotBelowRoot_DoesNotCheckAnything()
    {
        var result = _sut.ContainsReparsePoint(SomeRootPath, @"C:\Output2\SomeName.dat");

        Assert.False(result);
        _fileSystemUtilsMock.VerifyNoOtherCalls();
    }

    private void LetBeReparsePoint(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.IsReparsePoint(path))
            .Returns(true);
}
