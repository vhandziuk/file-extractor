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

    [Theory]
    [InlineData(@"SomeName.dat,C:\Windows,")]
    [InlineData(@"SomeName.dat,\Windows,")]
    [InlineData(@"SomeName.dat,C:Windows,")]
    [InlineData(@"SomeName.dat,\\server\share,")]
    [InlineData(@"SomeName.dat,..,")]
    [InlineData(@"SomeName.dat,Test\..\..\Windows,")]
    [InlineData("SomeName.dat,Test/../..,")]
    [InlineData("SomeName.dat,Test:stream,")]
    [InlineData("SomeName.dat,Te|st,")]
    public void EnumerateEntries_DirectoryIsNotRelativeToDestination_ThrowsInvalidDataExceptionWithLineNumber(string line)
    {
        LetLinesBe("SomeName.dat,Test,SomeDirectory", line);

        var exception = Assert.Throws<InvalidDataException>(() => _sut.EnumerateEntries(SomeConfigurationPath).ToArray());

        Assert.Contains("line 2", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(@"Test\Nested")]
    [InlineData("Test/Nested")]
    [InlineData(@"Test\..Nested")]
    [InlineData(@"Test\Nested..")]
    public void EnumerateEntries_DirectoryIsRelative_ReturnsFileInfoData(string directory)
    {
        LetLinesBe($"SomeName.dat,{directory},");

        var result = _sut.EnumerateEntries(SomeConfigurationPath).ToArray();

        Assert.Equal([new FileInfoData(directory, "SomeName.dat", "")], result);
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
