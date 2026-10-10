using FileExtractor.Utils.Compression;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression;

public class ArchiveTypeResolverTest
{
    private readonly ArchiveTypeResolver _sut = new();

    [Theory]
    [InlineData(@"C:\Source\SomeArchive.zip", ArchiveType.Zip)]
    [InlineData(@"C:\Source\SomeArchive.ZIP", ArchiveType.Zip)]
    [InlineData(@"C:\Source\SomeArchive.rar", ArchiveType.Rar)]
    [InlineData(@"C:\Source\SomeArchive.7z", ArchiveType.SevenZip)]
    [InlineData(@"C:\Source\SomeArchive.tar", ArchiveType.Other)]
    [InlineData(@"C:\Source\SomeArchive.tar.gz", ArchiveType.Other)]
    [InlineData(@"C:\Source\SomeArchive.tar.bz2", ArchiveType.Other)]
    [InlineData(@"C:\Source\SomeArchive.tar.lz", ArchiveType.Other)]
    [InlineData(@"C:\Source\SomeArchive.tar.XZ", ArchiveType.Other)]
    public void TryResolve_ExtensionIsSupported_ReturnsArchiveType(string filePath, ArchiveType expectedArchiveType)
    {
        var result = _sut.TryResolve(filePath, out var archiveType);

        Assert.True(result);
        Assert.Equal(expectedArchiveType, archiveType);
    }

    [Theory]
    [InlineData(@"C:\Source\configuration.csv")]
    [InlineData(@"C:\Source\SomeArchive.zip.bak")]
    [InlineData(@"C:\Source\SomeFile")]
    [InlineData("")]
    public void TryResolve_ExtensionIsNotSupported_ReturnsFalse(string filePath)
    {
        var result = _sut.TryResolve(filePath, out _);

        Assert.False(result);
    }
}
