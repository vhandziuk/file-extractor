using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor.Application.Commands;
using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;
using Moq;
using Xunit;

namespace FileExtractor.UnitTest.Application.Commands;

public class ListCommandTest
{
    private const string SomeArchivePath = @"C:\Source";
    private const string SomeZipArchive = @"C:\Source\b.zip";
    private const string SomeRarArchive = @"C:\Source\a.rar";

    private readonly Mock<IArchiveFinder> _archiveFinderMock = new();
    private readonly Mock<IArchiveTypeResolver> _archiveTypeResolverMock = new();
    private readonly Mock<IArchiveExtractorFactory> _archiveExtractorFactoryMock = new();
    private readonly Mock<IConsole> _consoleMock = new();
    private readonly Mock<ILogger<ListCommand>> _loggerMock = new();
    private readonly Mock<IListOptions> _optionsMock = new();
    private readonly Mock<IArchiveFileExtractor> _zipFileExtractorMock = new();
    private readonly Mock<IArchiveFileExtractor> _rarFileExtractorMock = new();
    private readonly List<string> _output = [];

    private readonly ListCommand _sut;

    public ListCommandTest()
    {
        _optionsMock.SetupGet(options => options.Archive).Returns(SomeArchivePath);
        _consoleMock
            .Setup(console => console.WriteLine(It.IsAny<string>()))
            .Callback<string>(_output.Add);
        LetArchiveTypeBe(SomeZipArchive, ArchiveType.Zip, _zipFileExtractorMock);
        LetArchiveTypeBe(SomeRarArchive, ArchiveType.Rar, _rarFileExtractorMock);

        _sut = new ListCommand(
            _archiveFinderMock.Object,
            _archiveTypeResolverMock.Object,
            _archiveExtractorFactoryMock.Object,
            _consoleMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_NoArchivesFound_LogsWarningAndPrintsNothing()
    {
        LetArchivesBe();

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("No supported archive files found at {Path}. The program will now exit", SomeArchivePath), Times.Once);
        Assert.Empty(_output);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsync_RecursiveOptionProvided_SearchesArchivesWithIt(bool recursive)
    {
        _optionsMock.SetupGet(options => options.Recursive).Returns(recursive);

        await RunAsync();

        _archiveFinderMock.Verify(finder => finder.FindArchives(SomeArchivePath, recursive), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ArchivesFound_PrintsEachArchiveInNameOrderWithItsEntries()
    {
        LetArchivesBe(SomeZipArchive, SomeRarArchive);
        LetEntriesBe(_zipFileExtractorMock, SomeZipArchive, "SomeName1.dat", "SomeDirectory/SomeName2.dat");
        LetEntriesBe(_rarFileExtractorMock, SomeRarArchive, "SomeName3.dat");

        await RunAsync();

        Assert.Equal(
            [SomeRarArchive, "  SomeName3.dat", SomeZipArchive, "  SomeName1.dat", "  SomeDirectory/SomeName2.dat"],
            _output);
    }

    [Fact]
    public async Task RunAsync_ListingArchiveThrows_LogsErrorAndListsOtherArchives()
    {
        var expectedException = new InvalidDataException();
        LetArchivesBe(SomeZipArchive, SomeRarArchive);
        _rarFileExtractorMock.Setup(extractor => extractor.ListEntries(SomeRarArchive)).Throws(expectedException);
        LetEntriesBe(_zipFileExtractorMock, SomeZipArchive, "SomeName1.dat");

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "Failed to list files in {Archive}. Skipping it", SomeRarArchive), Times.Once);
        Assert.Equal([SomeZipArchive, "  SomeName1.dat"], _output);
    }

    [Fact]
    public async Task RunAsync_CancellationRequested_ThrowsBeforeListing()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        LetArchivesBe(SomeZipArchive);

        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.RunAsync(_optionsMock.Object, cancellationTokenSource.Token));

        Assert.Empty(_output);
    }

    private Task RunAsync() =>
        _sut.RunAsync(_optionsMock.Object, TestContext.Current.CancellationToken);

    private void LetArchivesBe(params string[] archives) =>
        _archiveFinderMock
            .Setup(finder => finder.FindArchives(SomeArchivePath, It.IsAny<bool>()))
            .Returns(archives);

    private void LetArchiveTypeBe(string archive, ArchiveType archiveType, Mock<IArchiveFileExtractor> extractorMock)
    {
        _archiveTypeResolverMock
            .Setup(resolver => resolver.TryResolve(archive, out archiveType))
            .Returns(true);
        _archiveExtractorFactoryMock
            .Setup(factory => factory.Create(archiveType))
            .Returns(extractorMock.Object);
    }

    private static void LetEntriesBe(Mock<IArchiveFileExtractor> extractorMock, string archive, params string[] entries) =>
        extractorMock
            .Setup(extractor => extractor.ListEntries(archive))
            .Returns(entries);
}
