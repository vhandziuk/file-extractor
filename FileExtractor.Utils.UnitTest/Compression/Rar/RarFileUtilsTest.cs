using FileExtractor.Utils.Compression.Rar;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Rar;

public class RarFileUtilsTest
{
    private readonly Mock<IRarFile> _rarFileMock = new();
    private readonly RarFileUtils _sut;

    public RarFileUtilsTest()
    {
        _sut = new RarFileUtils(_rarFileMock.Object);
    }

    [Fact]
    public void OpenRead_ProxiesCallToRarFile()
    {
        var someArchiveFileName = "SomeArchiveFileName.rar";

        _sut.OpenRead(someArchiveFileName);

        _rarFileMock.Verify(file =>
            file.OpenRead(someArchiveFileName), Times.Once);
    }
}