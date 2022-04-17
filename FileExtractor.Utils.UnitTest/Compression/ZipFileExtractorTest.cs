using System;
using System.IO;
using System.Threading.Tasks;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.FileSystem;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression;

public class ZipFileExtractorTest
{
    private const string SomeExtractedPath = @"C:\Extracted";
    private const string SomeArchiveFileName = @"C:\SomeArchive.zip";
    private const string AnotherArchiveFileName = @"C:\AnotherArchive.zip";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IZipFileUtils> _zipFileUtilsMock = new();
    private readonly Mock<ITaskRunner> _taskRunnerMock = new();
    private readonly Mock<ILogger<ZipFileExtractor>> _loggerMock = new();

    private readonly ZipFileExtractor _sut;

    public ZipFileExtractorTest()
    {
        _taskRunnerMock
            .Setup(runner => runner.Run(It.IsAny<Action>()))
            .Callback<Action>(action => action());

        _sut = new ZipFileExtractor(
            _fileSystemUtilsMock.Object,
            _zipFileUtilsMock.Object,
            _taskRunnerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExtractFiles_ArchivesContainNoEntries_LogsWarning()
    {
        LetZipArchiveEntriesBe(SomeArchiveFileName, Array.Empty<IZipArchiveEntry>());

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName },
            SomeExtractedPath,
            Array.Empty<FileInfoData>());

        _loggerMock.Verify(logger =>
            logger.Warning("The supplied archive(s) contain no entries"), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ExtractedPathDoesNotExist_CreatesExtractedPath()
    {
        var zipArchiveEntry = GetMockedZipArchiveEntry(
            "SomeName.dat", @"SomeDirectory\SomeName.dat").Object;
        LetZipArchiveEntriesBe(SomeArchiveFileName, zipArchiveEntry);
        LetDirectoryNotExist(SomeExtractedPath);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName },
            SomeExtractedPath,
            new[] { new FileInfoData("", "SomeName.dat", "SomeDirectory") });

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeExtractedPath), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ExtractedPathExists_DoesNotCreateExtractedPath()
    {
        var zipArchiveEntry = GetMockedZipArchiveEntry(
            "SomeName.dat", @"SomeDirectory\SomeName.dat").Object;
        LetZipArchiveEntriesBe(SomeArchiveFileName, zipArchiveEntry);
        LetDirectoryExist(SomeExtractedPath);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName },
            SomeExtractedPath,
            new[] { new FileInfoData("", "SomeName.dat", "SomeDirectory") });

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeExtractedPath), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesContainMatchingEntries_ExtractsEntries()
    {
        var _zipEntryMock = GetMockedZipArchiveEntry(
            "SomeName.dat", @"SomeDirectory\SomeName.dat");
        var _anotherZipEntryMock = GetMockedZipArchiveEntry(
            "AnotherName.dat", @"AnotherDirectory\AnotherName.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName, _zipEntryMock.Object);
        LetZipArchiveEntriesBe(AnotherArchiveFileName, _anotherZipEntryMock.Object);

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName,
                AnotherArchiveFileName
            },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("", "SomeName.dat", "SomeDirectory"),
                new FileInfoData("", "AnotherName.dat", "AnotherDirectory")
            });

        _loggerMock.Verify(logger =>
            logger.Information("Processing files"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Information(It.Is<string>(message => message.StartsWith("Extracting")), It.IsAny<string>(), SomeExtractedPath), Times.Exactly(2));
        _loggerMock.Verify(logger =>
            logger.Information("Processing completed. All files have been successfully extracted"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(logger =>
            logger.Warning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        _zipEntryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName.dat"), true), Times.Once);
        _anotherZipEntryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "AnotherName.dat"), true), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesDoNotContainMatchingEntries_DoesNotExtractEntries()
    {
        var _zipEntryMock = GetMockedZipArchiveEntry(
            "SomeName.dat", @"SomeDirectory\SomeName.dat");
        var _anotherZipEntryMock = GetMockedZipArchiveEntry(
            "AnotherName.dat", @"AnotherDirectory\AnotherName.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName, _zipEntryMock.Object);
        LetZipArchiveEntriesBe(AnotherArchiveFileName, _anotherZipEntryMock.Object);

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName,
                AnotherArchiveFileName
            },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("", "NonMatchingName.dat", "SomeDirectory"),
                new FileInfoData("", "AnotherNonMatchingName.dat", "AnotherDirectory")
            });

        _loggerMock.Verify(logger =>
            logger.Information("Processing files"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning("Processing completed. Missing files detected"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning(It.Is<string>(message => message.EndsWith("was not found in the supplied archive(s)")), It.IsAny<object[]>()), Times.Exactly(2));
        _zipEntryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), true), Times.Never);
        _anotherZipEntryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), true), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesContainNoMatchingEntriesByFileName_DoesNotExtractEntries()
    {
        var _zipEntryMock = GetMockedZipArchiveEntry(
            "SomeOtherName.dat", @"SomeDirectory\SomeOtherName.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName, _zipEntryMock.Object);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName },
            SomeExtractedPath,
            new[] { new FileInfoData("", "SomeName.dat", "SomeDirectory") });

        _zipEntryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName.dat"), true), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesContainNoMatchingEntriesByParentDirectory_DoesNotExtractEntries()
    {
        var _zipEntryMock = GetMockedZipArchiveEntry(
            "SomeName.dat", @"SomeOtherDirectory\SomeName.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName, _zipEntryMock.Object);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName },
            SomeExtractedPath,
            new[] { new FileInfoData("", "SomeName.dat", "SomeDirectory") });

        _zipEntryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName.dat"), true), Times.Never);
    }

    private Mock<IZipArchiveEntry> GetMockedZipArchiveEntry(string name, string fullName)
    {
        var mock = new Mock<IZipArchiveEntry>();
        mock.SetupGet(entry => entry.Name).Returns(name);
        mock.SetupGet(entry => entry.FullName).Returns(fullName);

        return mock;
    }

    private void LetZipArchiveEntriesBe(string archiveFileName, params IZipArchiveEntry[] entries) =>
        _zipFileUtilsMock
            .Setup(utils => utils.OpenRead(archiveFileName))
            .Returns(Mock.Of<IZipArchive>(archive => archive.Entries == entries));

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);

    private void LetDirectoryNotExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(false);
}