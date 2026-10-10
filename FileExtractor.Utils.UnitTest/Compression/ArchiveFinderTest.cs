using System.IO;
using FileExtractor.Utils.Compression;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression;

public class ArchiveFinderTest
{
    private const string SomeDirectory = @"C:\Source";
    private const string SomeArchive = @"C:\Source\SomeArchive.zip";
    private const string SomeOtherArchive = @"C:\Source\SomeOtherArchive.zip";
    private const string SomeTextFile = @"C:\Source\SomeFile.txt";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IArchiveTypeResolver> _archiveTypeResolverMock = new();

    private readonly ArchiveFinder _sut;

    public ArchiveFinderTest()
    {
        LetArchiveExtensionBeSupported(".zip");

        _sut = new ArchiveFinder(_fileSystemUtilsMock.Object, _archiveTypeResolverMock.Object);
    }

    [Fact]
    public void FindArchives_PathIsSupportedArchive_ReturnsIt()
    {
        LetFileExist(SomeArchive);

        var result = _sut.FindArchives(SomeArchive, false);

        Assert.Equal([SomeArchive], result);
    }

    [Fact]
    public void FindArchives_PathIsUnsupportedFile_ReturnsEmpty()
    {
        LetFileExist(SomeTextFile);

        var result = _sut.FindArchives(SomeTextFile, false);

        Assert.Empty(result);
    }

    [Fact]
    public void FindArchives_PathDoesNotExist_ReturnsEmpty()
    {
        var result = _sut.FindArchives(SomeArchive, false);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(false, SearchOption.TopDirectoryOnly)]
    [InlineData(true, SearchOption.AllDirectories)]
    public void FindArchives_PathIsDirectory_ReturnsSupportedArchivesInScope(bool recursive, SearchOption expectedSearchOption)
    {
        LetDirectoryExist(SomeDirectory);
        LetGetFilesReturn(SomeDirectory, "*.*", expectedSearchOption, SomeArchive, SomeTextFile, SomeOtherArchive);

        var result = _sut.FindArchives(SomeDirectory, recursive);

        Assert.Equal([SomeArchive, SomeOtherArchive], result);
    }

    [Fact]
    public void FindArchives_DirectoryContainsOnlyUnsupportedFiles_ReturnsEmpty()
    {
        LetDirectoryExist(SomeDirectory);
        LetGetFilesReturn(SomeDirectory, "*.*", SearchOption.TopDirectoryOnly, SomeTextFile, @"C:\Source\configuration.csv");

        var result = _sut.FindArchives(SomeDirectory, false);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(false, SearchOption.TopDirectoryOnly)]
    [InlineData(true, SearchOption.AllDirectories)]
    public void FindArchives_PathContainsWildcard_ReturnsMatchingSupportedArchivesInScope(bool recursive, SearchOption expectedSearchOption)
    {
        const string someArchiveWithLongerExtension = @"C:\Source\SomeArchive.zipx";
        LetDirectoryExist(SomeDirectory);
        LetGetFilesReturn(SomeDirectory, "Some*.zip", expectedSearchOption, SomeArchive, someArchiveWithLongerExtension);

        var result = _sut.FindArchives(@"C:\Source\Some*.zip", recursive);

        Assert.Equal([SomeArchive], result);
    }

    [Fact]
    public void FindArchives_WildcardDirectoryDoesNotExist_ReturnsEmpty()
    {
        var result = _sut.FindArchives(@"C:\Missing\*.zip", false);

        Assert.Empty(result);
        _fileSystemUtilsMock.Verify(utils =>
            utils.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()), Times.Never);
    }

    private void LetArchiveExtensionBeSupported(string extension)
    {
        var archiveType = ArchiveType.Zip;
        _archiveTypeResolverMock
            .Setup(resolver => resolver.TryResolve(It.Is<string>(path => path.EndsWith(extension)), out archiveType))
            .Returns(true);
    }

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);

    private void LetFileExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(path))
            .Returns(true);

    private void LetGetFilesReturn(string path, string searchPattern, SearchOption searchOption, params string[] filePaths) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.GetFiles(path, searchPattern, searchOption))
            .Returns(filePaths);
}
