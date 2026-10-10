using System;
using System.IO;
using System.Linq;
using Moq;
using Xunit;

namespace FileExtractor.Data.UnitTest;

public class CsvFileInfoProviderTest
{
    private const string SomeConfigurationPath = @"C:\Source\configuration.csv";

    private readonly Mock<ITextFile> _textFileMock = new();

    private readonly CsvFileInfoProvider _sut;

    public CsvFileInfoProviderTest()
    {
        _sut = new CsvFileInfoProvider(_textFileMock.Object);
    }

    [Fact]
    public void EnumerateEntries_LinesAreValid_ReturnsFileInfoDataForEveryLine()
    {
        LetLinesBe(
            "SomeName1.dat,Test,SomeDirectory",
            "SomeName?.dat,,",
            @"*.txt,Test\Nested,SomeDirectory\Nested");

        var result = _sut.EnumerateEntries(SomeConfigurationPath).ToArray();

        Assert.Equal(
            [
                new FileInfoData("Test", "SomeName1.dat", "SomeDirectory"),
                new FileInfoData("", "SomeName?.dat", ""),
                new FileInfoData(@"Test\Nested", "*.txt", @"SomeDirectory\Nested")
            ],
            result);
    }

    [Fact]
    public void EnumerateEntries_FileIsEmpty_ReturnsNoEntries()
    {
        LetLinesBe();

        var result = _sut.EnumerateEntries(SomeConfigurationPath);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("SomeName.dat,Test")]
    [InlineData("SomeName.dat,Test,SomeDirectory,Extra")]
    [InlineData("")]
    public void EnumerateEntries_LineHasWrongColumnCount_ThrowsInvalidDataExceptionWithLineNumber(string malformedLine)
    {
        LetLinesBe("SomeName.dat,Test,SomeDirectory", malformedLine);

        var exception = Assert.Throws<InvalidDataException>(() => _sut.EnumerateEntries(SomeConfigurationPath).ToArray());

        Assert.Contains("line 2", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(",Test,SomeDirectory")]
    [InlineData("   ,Test,SomeDirectory")]
    public void EnumerateEntries_NameIsEmptyOrWhitespace_ThrowsInvalidDataExceptionWithLineNumber(string line)
    {
        LetLinesBe(line);

        var exception = Assert.Throws<InvalidDataException>(() => _sut.EnumerateEntries(SomeConfigurationPath).ToArray());

        Assert.Contains("line 1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnumerateEntries_EntriesNotEnumerated_DoesNotReadFile()
    {
        _sut.EnumerateEntries(SomeConfigurationPath);

        _textFileMock.VerifyNoOtherCalls();
    }

    private void LetLinesBe(params string[] lines) =>
        _textFileMock
            .Setup(file => file.ReadLines(SomeConfigurationPath))
            .Returns(lines);
}
