using FileExtractor.Application.Commands;
using FileExtractor.Common.Logging;
using FileExtractor.Utils;
using Moq;
using Xunit;

namespace FileExtractor.UnitTest.Application.Commands;

public class OutputPathResolverTest
{
    private const string SomeOutputPath = @"C:\Output";
    private const string SomeDefaultPath = @"C:\Default";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<ILogger<OutputPathResolver>> _loggerMock = new();

    private readonly OutputPathResolver _sut;

    public OutputPathResolverTest()
    {
        _sut = new OutputPathResolver(_fileSystemUtilsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Resolve_OutputPathProvided_ReturnsIt()
    {
        LetDirectoryExist(SomeOutputPath);

        var result = _sut.Resolve(SomeOutputPath, SomeDefaultPath);

        Assert.Equal(SomeOutputPath, result);
    }

    [Fact]
    public void Resolve_OutputPathNotProvided_ReturnsDefaultPath()
    {
        LetDirectoryExist(SomeDefaultPath);

        var result = _sut.Resolve(null, SomeDefaultPath);

        Assert.Equal(SomeDefaultPath, result);
    }

    [Fact]
    public void Resolve_OutputPathIsRelativeOrNotNormalized_ReturnsFullPath()
    {
        var result = _sut.Resolve(@"C:\Output\Nested\..", SomeDefaultPath);

        Assert.Equal(SomeOutputPath, result);
    }

    [Fact]
    public void Resolve_DirectoryExists_DoesNotCreateIt()
    {
        LetDirectoryExist(SomeOutputPath);

        _sut.Resolve(SomeOutputPath, SomeDefaultPath);

        _fileSystemUtilsMock.Verify(utils => utils.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Resolve_DirectoryDoesNotExist_CreatesIt()
    {
        _sut.Resolve(SomeOutputPath, SomeDefaultPath);

        _fileSystemUtilsMock.Verify(utils => utils.CreateDirectory(SomeOutputPath), Times.Once);
        _loggerMock.Verify(logger => logger.Information("Creating output directory {Path}", SomeOutputPath), Times.Once);
    }

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);
}
