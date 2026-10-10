using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.Compression.Common;
using FileExtractor.Utils.Compression.Other;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Other;

public class OtherArchiveFileExtractorTest
{
    private const string SomeArchiveFileName = @"C:\SomeArchive.tar.gz";

    private readonly Mock<IOtherArchiveFile> _otherArchiveFileMock = new();
    private readonly Mock<IArchiveEntryExtractor> _archiveEntryExtractorMock = new();
    private readonly IExtractionContext _context = Mock.Of<IExtractionContext>();

    private readonly OtherArchiveFileExtractor _sut;

    public OtherArchiveFileExtractorTest()
    {
        _sut = new OtherArchiveFileExtractor(
            _otherArchiveFileMock.Object,
            _archiveEntryExtractorMock.Object);
    }

    [Fact]
    public void ExtractFiles_ReaderHasEntries_PassesReaderEntriesToEntryExtractor()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        IGenericArchiveEntry[] entries = [Mock.Of<IGenericArchiveEntry>(), Mock.Of<IGenericArchiveEntry>()];
        LetArchiveEntriesBe(entries);
        List<IGenericArchiveEntry>? passedEntries = null;
        _archiveEntryExtractorMock
            .Setup(extractor => extractor.ExtractEntries(
                SomeArchiveFileName, It.IsAny<IEnumerable<IGenericArchiveEntry>>(), _context, cancellationTokenSource.Token))
            .Callback<string, IEnumerable<IGenericArchiveEntry>, IExtractionContext, CancellationToken>(
                (_, readerEntries, _, _) => passedEntries = readerEntries.ToList())
            .Returns([]);

        _sut.ExtractFiles(SomeArchiveFileName, _context, cancellationTokenSource.Token);

        Assert.Equal(entries, passedEntries);
    }

    [Fact]
    public void ExtractFiles_EntriesExtracted_ReturnsExtractedFiles()
    {
        LetArchiveEntriesBe();
        FileInfoData[] extractedFiles = [new("", "SomeName.dat", "SomeDirectory")];
        _archiveEntryExtractorMock
            .Setup(extractor => extractor.ExtractEntries(
                SomeArchiveFileName, It.IsAny<IEnumerable<IGenericArchiveEntry>>(), _context, It.IsAny<CancellationToken>()))
            .Returns(extractedFiles);

        var result = _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken);

        Assert.Same(extractedFiles, result);
    }

    [Fact]
    public void ExtractFiles_ArchiveProcessed_DisposesReader()
    {
        var readerMock = LetArchiveEntriesBe();

        _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken);

        readerMock.Verify(reader => reader.Dispose(), Times.Once);
    }

    [Fact]
    public void ExtractFiles_EntryExtractorThrows_DisposesReader()
    {
        var readerMock = LetArchiveEntriesBe();
        _archiveEntryExtractorMock
            .Setup(extractor => extractor.ExtractEntries(
                It.IsAny<string>(), It.IsAny<IEnumerable<IGenericArchiveEntry>>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Throws(new IOException());

        Assert.Throws<IOException>(() => _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken));

        readerMock.Verify(reader => reader.Dispose(), Times.Once);
    }

    [Fact]
    public void ExtractFiles_ArchiveCannotBeOpened_DoesNotExtractEntries()
    {
        _otherArchiveFileMock
            .Setup(file => file.OpenReader(SomeArchiveFileName))
            .Throws(new InvalidDataException());

        Assert.Throws<InvalidDataException>(() => _sut.ExtractFiles(SomeArchiveFileName, _context, TestContext.Current.CancellationToken));

        _archiveEntryExtractorMock.VerifyNoOtherCalls();
    }

    private Mock<IOtherArchiveReader> LetArchiveEntriesBe(params IGenericArchiveEntry[] entries)
    {
        var remainingEntries = new Queue<IGenericArchiveEntry>(entries);
        IGenericArchiveEntry? currentEntry = null;

        var readerMock = new Mock<IOtherArchiveReader>();
        readerMock
            .Setup(reader => reader.MoveToNextEntry())
            .Returns(() => remainingEntries.TryDequeue(out currentEntry));
        readerMock
            .SetupGet(reader => reader.Entry)
            .Returns(() => currentEntry!);

        _otherArchiveFileMock
            .Setup(file => file.OpenReader(SomeArchiveFileName))
            .Returns(readerMock.Object);

        return readerMock;
    }
}
