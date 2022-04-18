using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression.Zip;
using FileExtractor.Utils.FileSystem;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Zip;

public class ZipFileExtractorTest
{
    private const string SomeExtractedPath = @"C:\Extracted";
    private const string SomeArchiveFileName1 = @"C:\SomeArchive1.zip";
    private const string SomeArchiveFileName2 = @"C:\SomeArchive2.zip";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IZipFileUtils> _zipFileUtilsMock = new();
    private readonly Mock<ITaskRunner> _taskRunnerMock = new();
    private readonly Mock<ILogger<ZipFileExtractor>> _loggerMock = new();

    private readonly ZipFileExtractor _sut;

    public ZipFileExtractorTest()
    {
        _taskRunnerMock
            .Setup(runner => runner.Run(It.IsAny<Func<IEnumerable<FileInfoData>>>()))
            .Callback<Func<IEnumerable<FileInfoData>>>(func => func());

        _sut = new ZipFileExtractor(
            _fileSystemUtilsMock.Object,
            _zipFileUtilsMock.Object,
            _taskRunnerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExtractFiles_ArchivesContainNoEntries_LogsWarning()
    {
        LetZipArchiveEntriesBe(SomeArchiveFileName1, Array.Empty<IZipArchiveEntry>());

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeExtractedPath,
            Array.Empty<FileInfoData>());

        _loggerMock.Verify(logger => logger.Warning("The supplied archive(s) contain no entries"), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ExtractedPathsDoNotExist_CreatesExtractedPaths()
    {
        var zipArchiveEntry = GetMockedZipArchiveEntry(
            "SomeName1.dat", @"SomeDirectory\SomeName1.dat").Object;
        LetZipArchiveEntriesBe(SomeArchiveFileName1, zipArchiveEntry);
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
        var zipArchiveEntry = GetMockedZipArchiveEntry(
            "SomeName1.dat", @"SomeDirectory\SomeName1.dat").Object;
        LetZipArchiveEntriesBe(SomeArchiveFileName1, zipArchiveEntry);
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
    public async Task ExtractFiles_ZipArchivesContainMatchingEntries_ExtractsEntries()
    {
        var zipArchiveEntry1Mock = GetMockedZipArchiveEntry(
            "SomeName1.dat", @"SomeDirectory1\SomeName1.dat");
        var zipArchiveEntry2Mock = GetMockedZipArchiveEntry(
            "SomeName2.dat", @"SomeDirectory2\SomeName2.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName1, zipArchiveEntry1Mock.Object);
        LetZipArchiveEntriesBe(SomeArchiveFileName2, zipArchiveEntry2Mock.Object);

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
        zipArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName1.dat"), true), Times.Once);
        zipArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "Test", "SomeName2.dat"), true), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesDoNotContainMatchingEntries_DoesNotExtractEntries()
    {
        var zipArchiveEntry1Mock = GetMockedZipArchiveEntry(
            "SomeName1.dat", @"SomeDirectory1\SomeName1.dat");
        var zipArchiveEntry2Mock = GetMockedZipArchiveEntry(
            "SomeName2.dat", @"SomeDirectory2\SomeName2.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName1, zipArchiveEntry1Mock.Object);
        LetZipArchiveEntriesBe(SomeArchiveFileName2, zipArchiveEntry2Mock.Object);

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

        zipArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), true), Times.Never);
        zipArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), true), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesContainNoMatchingEntriesByFileName_DoesNotExtractEntries()
    {
        var zipArchiveEntry1Mock = GetMockedZipArchiveEntry(
            "SomeOtherName1.dat", @"SomeDirectory\SomeOtherName1.dat");
        var zipArchiveEntry2Mock = GetMockedZipArchiveEntry(
            "SomeOtherName2.dat", @"SomeDirectory\SomeOtherName2.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName1, zipArchiveEntry1Mock.Object);
        LetZipArchiveEntriesBe(SomeArchiveFileName2, zipArchiveEntry2Mock.Object);

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

        zipArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName1.dat"), true), Times.Never);
        zipArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "Test", "SomeName2.dat"), true), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_ZipArchivesContainNoMatchingEntriesByParentDirectory_DoesNotExtractEntries()
    {
        var zipArchiveEntry1Mock = GetMockedZipArchiveEntry(
            "SomeName1.dat", @"SomeOtherDirectory\SomeName1.dat");
        var zipArchiveEntry2Mock = GetMockedZipArchiveEntry(
            "SomeName2.dat", @"SomeOtherDirectory\SomeName2.dat");
        LetZipArchiveEntriesBe(SomeArchiveFileName1, zipArchiveEntry1Mock.Object);
        LetZipArchiveEntriesBe(SomeArchiveFileName2, zipArchiveEntry2Mock.Object);

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

        zipArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "SomeName1.dat"), true), Times.Never);
        zipArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeExtractedPath, "Test", "SomeName2.dat"), true), Times.Never);
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