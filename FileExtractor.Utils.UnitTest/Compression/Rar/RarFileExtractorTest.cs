using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.Compression.Common;
using FileExtractor.Utils.Compression.Rar;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Rar;

public class RarFileExtractorTest
{
    private const string SomeArchiveFileName = @"C:\SomeArchive.rar";

    private readonly Mock<IRarFileUtils> _fileUtilsMock = new();
    private readonly Mock<IArchiveEntryExtractor> _archiveEntryExtractorMock = new();
    private readonly Mock<IGenericArchive> _archiveMock = new();
    private readonly IExtractionContext _context = Mock.Of<IExtractionContext>();

    private readonly RarFileExtractor _sut;

    public RarFileExtractorTest()
    {
        _fileUtilsMock
            .Setup(utils => utils.OpenRead(SomeArchiveFileName))
            .Returns(_archiveMock.Object);

        _sut = new RarFileExtractor(
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
