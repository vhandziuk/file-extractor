using FileExtractor.Utils.Compression.Zip;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Zip;

public class ZipFileUtilsTest
{
    private readonly Mock<IZipFile> _zipFileMock = new();
    private readonly ZipFileUtils _sut;

    public ZipFileUtilsTest()
    {
        _sut = new ZipFileUtils(_zipFileMock.Object);
    }

    [Fact]
    public void OpenRead_ProxiesCallToZipFile()
    {
        var someArchiveFileName = "SomeArchiveFileName.zip";

        _sut.OpenRead(someArchiveFileName);

        _zipFileMock.Verify(file =>
            file.OpenRead(someArchiveFileName), Times.Once);
    }
}