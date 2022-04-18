using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.FileSystem;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Rar;

public class RarFileExtractorTest
{
    private const string SomeExtractedPath = @"C:\Extracted";
    private const string SomeArchiveFileName1 = @"C:\SomeArchive1.rar";
    private const string SomeArchiveFileName2 = @"C:\SomeArchive2.rar";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IRarFileUtils> _rarFileUtilsMock = new();
    private readonly Mock<ITaskRunner> _taskRunnerMock = new();
    private readonly Mock<ILogger<RarFileExtractor>> _loggerMock = new();

    private readonly RarFileExtractor _sut;

    public RarFileExtractorTest()
    {
        _taskRunnerMock
            .Setup(runner => runner.Run(It.IsAny<Func<IEnumerable<FileInfoData>>>()))
            .Callback<Func<IEnumerable<FileInfoData>>>(func => func());

        _sut = new RarFileExtractor(
            _fileSystemUtilsMock.Object,
            _rarFileUtilsMock.Object,
            _taskRunnerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExtractFiles_ArchivesContainNoEntries_LogsWarning()
    {
        LetRarArchiveEntriesBe(SomeArchiveFileName1, Array.Empty<IRarArchiveEntry>());

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeExtractedPath,
            Array.Empty<FileInfoData>());

        _loggerMock.Verify(logger => logger.Warning("The supplied archive(s) contain no entries"), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ExtractedPathsDoNotExist_CreatesExtractedPaths()
    {
        var rarArchiveEntry = GetMockedRarArchiveEntry(
            "SomeName1.dat", @"SomeDirectory\SomeName1.dat").Object;
        LetRarArchiveEntriesBe(SomeArchiveFileName1, rarArchiveEntry);
        LetDirectoryNotExist(SomeExtractedPath);
        LetDirectoryNotExist(Path.Combine(SomeExtractedPath, "Test"));

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("Test", "SomeName1.dat", "SomeDirectory")
            });

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeExtractedPath), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(Path.Combine(SomeExtractedPath, "Test")), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ExtractedPathExists_DoesNotCreateExtractedPaths()
    {
        var rarArchiveEntry = GetMockedRarArchiveEntry(
            "SomeName1.dat", @"SomeDirectory\SomeName1.dat").Object;
        LetRarArchiveEntriesBe(SomeArchiveFileName1, rarArchiveEntry);
        LetDirectoryExist(SomeExtractedPath);
        LetDirectoryExist(Path.Combine(SomeExtractedPath, "Test"));

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("Test", "SomeName1.dat", "SomeDirectory")
            });

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeExtractedPath), Times.Never);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(Path.Combine(SomeExtractedPath, "Test")), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_RarArchivesContainMatchingEntries_ExtractsEntries()
    {
        var rarArchiveEntry1Mock = GetMockedRarArchiveEntry(
            "SomeName1.dat", @"SomeDirectory1\SomeName1.dat");
        var rarArchiveEntry2Mock = GetMockedRarArchiveEntry(
            "SomeName2.dat", @"SomeDirectory2\SomeName2.dat");
        LetRarArchiveEntriesBe(SomeArchiveFileName1, rarArchiveEntry1Mock.Object);
        LetRarArchiveEntriesBe(SomeArchiveFileName2, rarArchiveEntry2Mock.Object);

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName1,
                SomeArchiveFileName2
            },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("", "SomeName1.dat", "SomeDirectory1"),
                new FileInfoData("Test", "SomeName2.dat", "SomeDirectory2")
            });

        _loggerMock.Verify(logger =>
            logger.Information(It.Is<string>(message => message.StartsWith("Extracting")), It.IsAny<string>(), SomeExtractedPath), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Information(It.Is<string>(message => message.StartsWith("Extracting")), It.IsAny<string>(), Path.Combine(SomeExtractedPath, "Test")), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(logger =>
            logger.Warning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        rarArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName1.dat"), true), Times.Once);
        rarArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "Test", "SomeName2.dat"), true), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_RarArchivesDoNotContainMatchingEntries_DoesNotExtractEntries()
    {
        var rarArchiveEntry1Mock = GetMockedRarArchiveEntry(
            "SomeName1.dat", @"SomeDirectory1\SomeName1.dat");
        var rarArchiveEntry2Mock = GetMockedRarArchiveEntry(
            "SomeName2.dat", @"SomeDirectory2\SomeName2.dat");
        LetRarArchiveEntriesBe(SomeArchiveFileName1, rarArchiveEntry1Mock.Object);
        LetRarArchiveEntriesBe(SomeArchiveFileName2, rarArchiveEntry2Mock.Object);

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName1,
                SomeArchiveFileName2
            },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("", "NonMatchingName1.dat", "SomeDirectory1"),
                new FileInfoData("Test", "NonMatchingName2.dat", "SomeDirectory2")
            });

        rarArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), true), Times.Never);
        rarArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), true), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_RarArchivesContainNoMatchingEntriesByFileName_DoesNotExtractEntries()
    {
        var rarArchiveEntry1Mock = GetMockedRarArchiveEntry(
            "SomeOtherName1.dat", @"SomeDirectory\SomeOtherName1.dat");
        var rarArchiveEntry2Mock = GetMockedRarArchiveEntry(
            "SomeOtherName2.dat", @"SomeDirectory\SomeOtherName2.dat");
        LetRarArchiveEntriesBe(SomeArchiveFileName1, rarArchiveEntry1Mock.Object);
        LetRarArchiveEntriesBe(SomeArchiveFileName2, rarArchiveEntry2Mock.Object);

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName1,
                SomeArchiveFileName2
            },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("", "SomeName1.dat", "SomeDirectory"),
                new FileInfoData("Test", "SomeName2.dat", "SomeDirectory")
            });

        rarArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName1.dat"), true), Times.Never);
        rarArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "Test", "SomeName2.dat"), true), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_RarArchivesContainNoMatchingEntriesByParentDirectory_DoesNotExtractEntries()
    {
        var rarArchiveEntry1Mock = GetMockedRarArchiveEntry(
            "SomeName1.dat", @"SomeOtherDirectory\SomeName1.dat");
        var rarArchiveEntry2Mock = GetMockedRarArchiveEntry(
            "SomeName2.dat", @"SomeOtherDirectory\SomeName2.dat");
        LetRarArchiveEntriesBe(SomeArchiveFileName1, rarArchiveEntry1Mock.Object);
        LetRarArchiveEntriesBe(SomeArchiveFileName2, rarArchiveEntry2Mock.Object);

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName1,
                SomeArchiveFileName2
            },
            SomeExtractedPath,
            new[]
            {
                new FileInfoData("", "SomeName1.dat", "SomeDirectory"),
                new FileInfoData("Test", "SomeName2.dat", "SomeDirectory")
            });

        rarArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName1.dat"), true), Times.Never);
        rarArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "Test", "SomeName2.dat"), true), Times.Never);
    }

    private Mock<IRarArchiveEntry> GetMockedRarArchiveEntry(string name, string fullName)
    {
        var mock = new Mock<IRarArchiveEntry>();
        mock.SetupGet(entry => entry.Name).Returns(name);
        mock.SetupGet(entry => entry.FullName).Returns(fullName);

        return mock;
    }

    private void LetRarArchiveEntriesBe(string archiveFileName, params IRarArchiveEntry[] entries) =>
        _rarFileUtilsMock
            .Setup(utils => utils.OpenRead(archiveFileName))
            .Returns(Mock.Of<IRarArchive>(archive => archive.Entries == entries));

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);

    private void LetDirectoryNotExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(false);
}