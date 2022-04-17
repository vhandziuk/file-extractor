using FileExtractor.Utils.Compression.Rar;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Rar;

public class RarFileUtilsTest
{
    private readonly Mock<IRarFile> _zipFileMock = new();
    private readonly RarFileUtils _sut;

    public RarFileUtilsTest()
    {
        _sut = new RarFileUtils(_zipFileMock.Object);
    }

    [Fact]
    public void OpenRead_ProxiesCallToRarFile()
    {
        var someArchiveFileName = "SomeArchiveFileName.rar";

        _sut.OpenRead(someArchiveFileName);

        _zipFileMock.Verify(file =>
            file.OpenRead(someArchiveFileName), Times.Once);
    }
}