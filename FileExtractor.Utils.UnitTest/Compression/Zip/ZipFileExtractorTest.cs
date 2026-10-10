using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.Compression.Common;
using FileExtractor.Utils.Compression.Zip;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Zip;

public class ZipFileExtractorTest
{
    private const string SomeArchiveFileName = @"C:\SomeArchive.zip";

    private readonly Mock<IZipFileUtils> _fileUtilsMock = new();
    private readonly Mock<IArchiveEntryExtractor> _archiveEntryExtractorMock = new();
    private readonly Mock<IGenericArchive> _archiveMock = new();
    private readonly IExtractionContext _context = Mock.Of<IExtractionContext>();

    private readonly ZipFileExtractor _sut;

    public ZipFileExtractorTest()
    {
        _fileUtilsMock
            .Setup(utils => utils.OpenRead(SomeArchiveFileName))
            .Returns(_archiveMock.Object);

        _sut = new ZipFileExtractor(
            _fileUtilsMock.Object,
            _archiveEntryExtractorMock.Object);
    }

    [Fact]
    public void ExtractFiles_ArchiveContainsEntries_PassesEntriesToEntryExtractor()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        IGenericArchiveEntry[] entries = [Mock.Of<IGenericArchiveEntry>(), Mock.Of<IGenericArchiveEntry>()];
        LetArchiveEntriesBe(entries);

        _sut.ExtractFiles(SomeArchiveFileName, _context, cancellationTokenSource.Token);

        _archiveEntryExtractorMock.Verify(extractor =>
            extractor.ExtractEntries(SomeArchiveFileName, entries, _context, cancellationTokenSource.Token), Times.Once);
    }

    [Fact]
    public void ExtractFiles_EntriesExtracted_ReturnsExtractedFiles()
    {
        FileInfoData[] extractedFiles = [new("", "SomeName.dat", "SomeDirectory")];
        LetExtractedFilesBe(extractedFiles);

        var result = _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken);

        Assert.Same(extractedFiles, result);
    }

    [Fact]
    public void ExtractFiles_ArchiveProcessed_DisposesArchive()
    {
        _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken);

        _archiveMock.Verify(archive => archive.Dispose(), Times.Once);
    }

    [Fact]
    public void ExtractFiles_EntryExtractorThrows_DisposesArchive()
    {
        _archiveEntryExtractorMock
            .Setup(extractor => extractor.ExtractEntries(
                It.IsAny<string>(), It.IsAny<IEnumerable<IGenericArchiveEntry>>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Throws(new IOException());

        Assert.Throws<IOException>(() => _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken));

        _archiveMock.Verify(archive => archive.Dispose(), Times.Once);
    }

    [Fact]
    public void ExtractFiles_ArchiveCannotBeOpened_DoesNotExtractEntries()
    {
        _fileUtilsMock
            .Setup(utils => utils.OpenRead(SomeArchiveFileName))
            .Throws(new InvalidDataException());

        Assert.Throws<InvalidDataException>(() => _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken));

        _archiveEntryExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void ListEntries_ArchiveContainsEntries_ReturnsTheirFullNamesAndDisposesArchive()
    {
        LetArchiveEntriesBe(
            GetMockedGenericArchiveEntry("SomeName1.dat", "SomeName1.dat"),
            GetMockedGenericArchiveEntry("SomeName2.dat", "SomeDirectory/SomeName2.dat"));

        var result = _sut.ListEntries(SomeArchiveFileName);

        Assert.Equal(["SomeName1.dat", "SomeDirectory/SomeName2.dat"], result);
        _archiveMock.Verify(archive => archive.Dispose(), Times.Once);
        _archiveEntryExtractorMock.VerifyNoOtherCalls();
    }

    private static IGenericArchiveEntry GetMockedGenericArchiveEntry(string name, string fullName) =>
        Mock.Of<IGenericArchiveEntry>(entry => entry.Name == name && entry.FullName == fullName);

    private void LetArchiveEntriesBe(params IGenericArchiveEntry[] entries) =>
        _archiveMock
            .SetupGet(archive => archive.Entries)
            .Returns(entries);

    private void LetExtractedFilesBe(params FileInfoData[] extractedFiles) =>
        _archiveEntryExtractorMock
            .Setup(extractor => extractor.ExtractEntries(
                SomeArchiveFileName, It.IsAny<IEnumerable<IGenericArchiveEntry>>(), _context, It.IsAny<CancellationToken>()))
            .Returns(extractedFiles);
}
