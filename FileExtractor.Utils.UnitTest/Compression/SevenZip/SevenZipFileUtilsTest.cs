using FileExtractor.Utils.Compression.SevenZip;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.SevenZip;

public class SevenZipFileUtilsTest
{
    private readonly Mock<ISevenZipFile> _sevenZipFileMock = new();
    private readonly SevenZipFileUtils _sut;

    public SevenZipFileUtilsTest()
    {
        _sut = new SevenZipFileUtils(_sevenZipFileMock.Object);
    }

    [Fact]
    public void OpenRead_ProxiesCallToSevenZipFile()
    {
        var someArchiveFileName = "SomeArchiveFileName.7z";

        _sut.OpenRead(someArchiveFileName);

        _sevenZipFileMock.Verify(file =>
            file.OpenRead(someArchiveFileName), Times.Once);
    }
}