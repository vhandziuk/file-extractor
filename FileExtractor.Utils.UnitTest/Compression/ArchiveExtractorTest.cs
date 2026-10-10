using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression;

public class ArchiveExtractorTest
{
    private const string SomeOutputPath = @"C:\Output";
    private const string SomeZipArchive = @"C:\Source\SomeArchive.zip";
    private const string SomeRarArchive = @"C:\Source\SomeArchive.rar";
    private const int SomeMaxDegreeOfParallelism = 3;

    private static readonly FileInfoData SomeFileInfo1 = new("", "SomeName1.dat", "SomeDirectory");
    private static readonly FileInfoData SomeFileInfo2 = new("Test", "SomeName2.dat", "SomeDirectory");

    private readonly Mock<IArchiveExtractorFactory> _archiveExtractorFactoryMock = new();
    private readonly Mock<IArchiveTypeResolver> _archiveTypeResolverMock = new();
    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<ITaskRunner> _taskRunnerMock = new();
    private readonly Mock<ILogger<ArchiveExtractor>> _loggerMock = new();
    private readonly Mock<IArchiveFileExtractor> _zipFileExtractorMock = new();
    private readonly Mock<IArchiveFileExtractor> _rarFileExtractorMock = new();

    private readonly ArchiveExtractor _sut;

    public ArchiveExtractorTest()
    {
        LetTaskRunnerRunSequentially();
        LetArchiveTypeBe(SomeZipArchive, ArchiveType.Zip);
        LetArchiveTypeBe(SomeRarArchive, ArchiveType.Rar);
        LetExtractorBe(ArchiveType.Zip, _zipFileExtractorMock);
        LetExtractorBe(ArchiveType.Rar, _rarFileExtractorMock);

        _sut = new ArchiveExtractor(
            _archiveExtractorFactoryMock.Object,
            _archiveTypeResolverMock.Object,
            _fileSystemUtilsMock.Object,
            _taskRunnerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExtractFilesAsync_ArchivesOfDifferentTypes_DispatchesEachArchiveToItsExtractor()
    {
        await ExtractFilesAsync([SomeZipArchive, SomeRarArchive], [SomeFileInfo1]);

        _zipFileExtractorMock.Verify(extractor =>
            extractor.ExtractFiles(SomeZipArchive, It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()), Times.Once);
        _rarFileExtractorMock.Verify(extractor =>
            extractor.ExtractFiles(SomeRarArchive, It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExtractFilesAsync_ArchivesProcessed_SharesOneContextWithOutputPathAndFiles()
    {
        var contexts = new List<IExtractionContext>();
        _zipFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(It.IsAny<string>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Callback<string, IExtractionContext, CancellationToken>((_, context, _) => contexts.Add(context))
            .Returns([]);
        _rarFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(It.IsAny<string>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Callback<string, IExtractionContext, CancellationToken>((_, context, _) => contexts.Add(context))
            .Returns([]);
        FileInfoData[] fileData = [SomeFileInfo1, SomeFileInfo2];

        await ExtractFilesAsync([SomeZipArchive, SomeRarArchive], fileData);

        Assert.Equal(2, contexts.Count);
        Assert.Same(contexts[0], contexts[1]);
        Assert.Equal(SomeOutputPath, contexts[0].OutputPath);
        Assert.Equal(fileData, contexts[0].Files);
        Assert.False(contexts[0].FollowSymbolicLinks);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExtractFilesAsync_FollowSymbolicLinksProvided_PassesItInContext(bool followSymbolicLinks)
    {
        IExtractionContext? passedContext = null;
        _zipFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(It.IsAny<string>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Callback<string, IExtractionContext, CancellationToken>((_, context, _) => passedContext = context)
            .Returns([]);

        await ExtractFilesAsync([SomeZipArchive], [SomeFileInfo1], followSymbolicLinks);

        Assert.NotNull(passedContext);
        Assert.Equal(followSymbolicLinks, passedContext.FollowSymbolicLinks);
    }

    [Fact]
    public async Task ExtractFilesAsync_MaxDegreeOfParallelismProvided_PassesItToTaskRunner()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await _sut.ExtractFilesAsync([SomeZipArchive], GetSettings([SomeFileInfo1]), cancellationTokenSource.Token);

        _taskRunnerMock.Verify(runner =>
            runner.ForEachAsync(
                It.IsAny<IEnumerable<string>>(),
                SomeMaxDegreeOfParallelism,
                It.IsAny<Func<string, CancellationToken, ValueTask>>(),
                cancellationTokenSource.Token), Times.Once);
    }

    [Fact]
    public async Task ExtractFilesAsync_ArchivesHaveDifferentSizes_ProcessesLargestArchiveFirst()
    {
        var processedArchives = new List<string>();
        _zipFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(It.IsAny<string>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Callback<string, IExtractionContext, CancellationToken>((archive, _, _) => processedArchives.Add(archive))
            .Returns([]);
        _rarFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(It.IsAny<string>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Callback<string, IExtractionContext, CancellationToken>((archive, _, _) => processedArchives.Add(archive))
            .Returns([]);
        LetFileSizeBe(SomeZipArchive, 10);
        LetFileSizeBe(SomeRarArchive, 1000);

        await ExtractFilesAsync([SomeZipArchive, SomeRarArchive], [SomeFileInfo1]);

        Assert.Equal([SomeRarArchive, SomeZipArchive], processedArchives);
    }

    [Fact]
    public async Task ExtractFilesAsync_ArchiveExtractionThrows_LogsErrorAndContinuesWithOtherArchives()
    {
        var expectedException = new InvalidDataException();
        LetFileSizeBe(SomeZipArchive, 1000);
        LetFileSizeBe(SomeRarArchive, 10);
        _zipFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(SomeZipArchive, It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Throws(expectedException);
        LetExtractedFilesBe(_rarFileExtractorMock, SomeRarArchive, SomeFileInfo2);

        var result = await ExtractFilesAsync([SomeZipArchive, SomeRarArchive], [SomeFileInfo1, SomeFileInfo2]);

        Assert.Equal([SomeFileInfo2], result);
        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "Failed to extract files from {Archive}. Skipping it", SomeZipArchive), Times.Once);
        _rarFileExtractorMock.Verify(extractor =>
            extractor.ExtractFiles(SomeRarArchive, It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExtractFilesAsync_ArchiveExtractionCancelled_PropagatesCancellation()
    {
        _zipFileExtractorMock
            .Setup(extractor => extractor.ExtractFiles(SomeZipArchive, It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Throws(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            ExtractFilesAsync([SomeZipArchive], [SomeFileInfo1]));

        _loggerMock.Verify(logger =>
            logger.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task ExtractFilesAsync_ArchiveTypeIsUnsupported_LogsWarningAndSkipsArchive()
    {
        const string someUnsupportedArchive = @"C:\Source\SomeArchive.unknown";

        await ExtractFilesAsync([someUnsupportedArchive], [SomeFileInfo1]);

        _loggerMock.Verify(logger =>
            logger.Warning("Archive {Archive} has an unsupported type. Skipping it", someUnsupportedArchive), Times.Once);
        _archiveExtractorFactoryMock.Verify(factory =>
            factory.Create(It.IsAny<ArchiveType>()), Times.Never);
    }

    [Fact]
    public async Task ExtractFilesAsync_AllFilesExtracted_ReturnsExtractedFilesAndLogsSuccess()
    {
        LetExtractedFilesBe(_zipFileExtractorMock, SomeZipArchive, SomeFileInfo1);
        LetExtractedFilesBe(_rarFileExtractorMock, SomeRarArchive, SomeFileInfo1, SomeFileInfo2);

        var result = await ExtractFilesAsync([SomeZipArchive, SomeRarArchive], [SomeFileInfo1, SomeFileInfo2]);

        Assert.Equivalent(new[] { SomeFileInfo1, SomeFileInfo2 }, result);
        Assert.Equal(2, result.Count);
        _loggerMock.Verify(logger =>
            logger.Information("Processing completed. All files have been successfully extracted"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExtractFilesAsync_SomeFilesNotFound_LogsMissingFiles()
    {
        LetExtractedFilesBe(_zipFileExtractorMock, SomeZipArchive, SomeFileInfo1);

        await ExtractFilesAsync([SomeZipArchive], [SomeFileInfo1, SomeFileInfo2]);

        _loggerMock.Verify(logger =>
            logger.Warning("Processing completed. Missing files detected"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning("File {File} was not found in the supplied archive(s)", Path.Combine(SomeFileInfo2.Location, SomeFileInfo2.Name)), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning("File {File} was not found in the supplied archive(s)", Path.Combine(SomeFileInfo1.Location, SomeFileInfo1.Name)), Times.Never);
    }

    private Task<IReadOnlyCollection<FileInfoData>> ExtractFilesAsync(string[] archives, FileInfoData[] fileData, bool followSymbolicLinks = false) =>
        _sut.ExtractFilesAsync(archives, GetSettings(fileData) with { FollowSymbolicLinks = followSymbolicLinks }, TestContext.Current.CancellationToken);

    private static ExtractionSettings GetSettings(FileInfoData[] fileData) =>
        new(SomeOutputPath, fileData, SomeMaxDegreeOfParallelism);

    private void LetTaskRunnerRunSequentially() =>
        _taskRunnerMock
            .Setup(runner => runner.ForEachAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<int>(),
                It.IsAny<Func<string, CancellationToken, ValueTask>>(),
                It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<string>, int, Func<string, CancellationToken, ValueTask>, CancellationToken>(
                async (source, _, body, cancellationToken) =>
                {
                    foreach (var item in source)
                        await body(item, cancellationToken);
                });

    private void LetArchiveTypeBe(string archiveFile, ArchiveType archiveType) =>
        _archiveTypeResolverMock
            .Setup(resolver => resolver.TryResolve(archiveFile, out archiveType))
            .Returns(true);

    private void LetExtractorBe(ArchiveType archiveType, Mock<IArchiveFileExtractor> extractorMock)
    {
        extractorMock
            .Setup(extractor => extractor.ExtractFiles(It.IsAny<string>(), It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Returns([]);
        _archiveExtractorFactoryMock
            .Setup(factory => factory.Create(archiveType))
            .Returns(extractorMock.Object);
    }

    private static void LetExtractedFilesBe(Mock<IArchiveFileExtractor> extractorMock, string archiveFile, params FileInfoData[] extractedFiles) =>
        extractorMock
            .Setup(extractor => extractor.ExtractFiles(archiveFile, It.IsAny<IExtractionContext>(), It.IsAny<CancellationToken>()))
            .Returns(extractedFiles);

    private void LetFileSizeBe(string filePath, long size) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.GetFileSize(filePath))
            .Returns(size);
}
