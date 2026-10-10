using System;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.Compression.Other;
using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.Compression.SevenZip;
using FileExtractor.Utils.Compression.Zip;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression;

public class ArchiveExtractorFactoryTest
{
    private readonly Mock<IZipFileExtractor> _zipFileExtractorMock = new();
    private readonly Mock<IRarFileExtractor> _rarFileExtractorMock = new();
    private readonly Mock<ISevenZipFileExtractor> _sevenZipFileExtractorMock = new();
    private readonly Mock<IOtherArchiveFileExtractor> _otherArchiveFileExtractorMock = new();

    private readonly ArchiveExtractorFactory _sut;

    public ArchiveExtractorFactoryTest()
    {
        _sut = new ArchiveExtractorFactory(
            _zipFileExtractorMock.Object,
            _rarFileExtractorMock.Object,
            _sevenZipFileExtractorMock.Object,
            _otherArchiveFileExtractorMock.Object);
    }

    [Theory]
    [InlineData(ArchiveType.Zip)]
    [InlineData(ArchiveType.Rar)]
    [InlineData(ArchiveType.SevenZip)]
    [InlineData(ArchiveType.Other)]
    public void Create_ArchiveTypeIsSupported_ReturnsExtractorForArchiveType(ArchiveType archiveType)
    {
        var result = _sut.Create(archiveType);

        Assert.Same(GetExpectedExtractor(archiveType), result);
    }

    [Fact]
    public void Create_ArchiveTypeIsUndefined_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Create((ArchiveType)42));
    }

    private IArchiveFileExtractor GetExpectedExtractor(ArchiveType archiveType) =>
        archiveType switch
        {
            ArchiveType.Zip => _zipFileExtractorMock.Object,
            ArchiveType.Rar => _rarFileExtractorMock.Object,
            ArchiveType.SevenZip => _sevenZipFileExtractorMock.Object,
            _ => _otherArchiveFileExtractorMock.Object
        };
}
