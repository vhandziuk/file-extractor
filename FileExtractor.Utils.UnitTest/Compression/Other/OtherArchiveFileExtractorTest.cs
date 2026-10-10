using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using FileExtractor.Data;
using FileExtractor.Utils.Compression.Common;
using FileExtractor.Utils.Compression.Other;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Other;

public class OtherArchiveFileExtractorTest
{
    private const string SomeOutputPath = @"C:\Output";
    private const string SomeArchiveFileName1 = @"C:\SomeArchive1.tar.gz";
    private const string SomeArchiveFileName2 = @"C:\SomeArchive2.tar.gz";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IOtherArchiveFile> _otherArchiveFileMock = new();
    private readonly Mock<ITaskRunner> _taskRunnerMock = new();
    private readonly Mock<ILogger<OtherArchiveFileExtractor>> _loggerMock = new();

    private readonly OtherArchiveFileExtractor _sut;

    private IEnumerable<FileInfoData> _extractedFiles = Array.Empty<FileInfoData>();

    public OtherArchiveFileExtractorTest()
    {
        _taskRunnerMock
            .Setup(runner => runner.Run(It.IsAny<Func<IEnumerable<FileInfoData>>>()))
            .Callback<Func<IEnumerable<FileInfoData>>>(func => _extractedFiles = func());

        _sut = new OtherArchiveFileExtractor(
            _fileSystemUtilsMock.Object,
            _otherArchiveFileMock.Object,
            _taskRunnerMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExtractFiles_OutputPathDoesNotExist_CreatesOutputPathWithoutExtractedSubfolder()
    {
        var genericArchiveEntryMock = GetMockedGenericArchiveEntry(
            "SomeName1.dat", "SomeDirectory/SomeName1.dat");
        LetArchiveEntriesBe(SomeArchiveFileName1, genericArchiveEntryMock.Object);
        LetDirectoryNotExist(SomeOutputPath);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[] { new FileInfoData("", "SomeName1.dat", "SomeDirectory") });

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeOutputPath), Times.AtLeastOnce);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(Path.Combine(SomeOutputPath, "Extracted")), Times.Never);
        genericArchiveEntryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName1.dat"), true), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_ArchivesContainMatchingEntries_ExtractsEntries()
    {
        var genericArchiveEntry1Mock = GetMockedGenericArchiveEntry(
            "SomeName1.dat", "SomeDirectory1/SomeName1.dat");
        var genericArchiveEntry2Mock = GetMockedGenericArchiveEntry(
            "SomeName2.dat", "SomeDirectory2/SomeName2.dat");
        LetArchiveEntriesBe(SomeArchiveFileName1, genericArchiveEntry1Mock.Object);
        LetArchiveEntriesBe(SomeArchiveFileName2, genericArchiveEntry2Mock.Object);
        var fileData = new[]
        {
            new FileInfoData("", "SomeName1.dat", "SomeDirectory1"),
            new FileInfoData("Test", "SomeName2.dat", "SomeDirectory2")
        };

        await _sut.ExtractFiles(
            new[]
            {
                SomeArchiveFileName1,
                SomeArchiveFileName2
            },
            SomeOutputPath,
            fileData);

        genericArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName1.dat"), true), Times.Once);
        genericArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test", "SomeName2.dat"), true), Times.Once);
        Assert.Equal(fileData.ToHashSet(), _extractedFiles.ToHashSet());
    }

    [Fact]
    public async Task ExtractFiles_FileNameContainsWildcards_ExtractsMatchingEntries()
    {
        var genericArchiveEntry1Mock = GetMockedGenericArchiveEntry(
            "SomeName1.dat", "SomeDirectory/SomeName1.dat");
        var genericArchiveEntry2Mock = GetMockedGenericArchiveEntry(
            "SomeName2.dat", "SomeDirectory/SomeName2.dat");
        var genericArchiveEntry3Mock = GetMockedGenericArchiveEntry(
            "SomeName3.txt", "SomeDirectory/SomeName3.txt");
        LetArchiveEntriesBe(
            SomeArchiveFileName1,
            genericArchiveEntry1Mock.Object,
            genericArchiveEntry2Mock.Object,
            genericArchiveEntry3Mock.Object);
        var fileInfo = new FileInfoData("", "SomeName?.dat", "SomeDirectory");

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[] { fileInfo });

        genericArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName1.dat"), true), Times.Once);
        genericArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName2.dat"), true), Times.Once);
        genericArchiveEntry3Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.Equal(new[] { fileInfo }, _extractedFiles);
    }

    [Fact]
    public async Task ExtractFiles_LocationIsEmpty_ExtractsMatchingEntriesFromAnyFolder()
    {
        var genericArchiveEntry1Mock = GetMockedGenericArchiveEntry(
            "SomeName1.dat", "SomeName1.dat");
        var genericArchiveEntry2Mock = GetMockedGenericArchiveEntry(
            "SomeName2.dat", "SomeDirectory/Nested/SomeName2.dat");
        LetArchiveEntriesBe(SomeArchiveFileName1, genericArchiveEntry1Mock.Object, genericArchiveEntry2Mock.Object);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[] { new FileInfoData("", "SomeName?.dat", "") });

        genericArchiveEntry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName1.dat"), true), Times.Once);
        genericArchiveEntry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName2.dat"), true), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_EntryMatchesMultipleDestinations_ExtractsEntryOnceAndCopiesIt()
    {
        var genericArchiveEntryMock = GetMockedGenericArchiveEntry(
            "SomeName1.dat", "SomeDirectory/SomeName1.dat");
        LetArchiveEntriesBe(SomeArchiveFileName1, genericArchiveEntryMock.Object);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[]
            {
                new FileInfoData("Test1", "SomeName1.dat", "SomeDirectory"),
                new FileInfoData("Test2", "*.dat", "SomeDirectory")
            });

        genericArchiveEntryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test1", "SomeName1.dat"), true), Times.Once);
        genericArchiveEntryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(
                Path.Combine(SomeOutputPath, "Test1", "SomeName1.dat"),
                Path.Combine(SomeOutputPath, "Test2", "SomeName1.dat"),
                true), Times.Once);
    }

    [Fact]
    public async Task ExtractFiles_FileAlreadyExistsInOutputPath_DoesNotExtractFile()
    {
        var genericArchiveEntryMock = GetMockedGenericArchiveEntry(
            "SomeName.dat", "SomeDirectory/SomeName.dat");
        LetArchiveEntriesBe(SomeArchiveFileName1, genericArchiveEntryMock.Object);
        LetFileExist(Path.Combine(SomeOutputPath, "SomeName.dat"));

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[] { new FileInfoData("", "SomeName.dat", "SomeDirectory") });

        _loggerMock.Verify(logger =>
            logger.Warning("File {File} already exists in {Path}. Skipping extraction", "SomeName.dat", SomeOutputPath), Times.Once);
        genericArchiveEntryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ExtractFiles_ArchivesContainNoMatchingEntriesByParentDirectory_DoesNotExtractEntries()
    {
        var genericArchiveEntryMock = GetMockedGenericArchiveEntry(
            "SomeName1.dat", "SomeOtherDirectory/SomeName1.dat");
        LetArchiveEntriesBe(SomeArchiveFileName1, genericArchiveEntryMock.Object);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[] { new FileInfoData("", "SomeName1.dat", "SomeDirectory") });

        genericArchiveEntryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.Empty(_extractedFiles);
    }

    [Fact]
    public async Task ExtractFiles_ArchivesProcessed_DisposesReaders()
    {
        var readerMock = LetArchiveEntriesBe(SomeArchiveFileName1);

        await _sut.ExtractFiles(
            new[] { SomeArchiveFileName1 },
            SomeOutputPath,
            new[] { new FileInfoData("", "SomeName1.dat", "") });

        readerMock.Verify(reader => reader.Dispose(), Times.Once);
    }

    private Mock<IGenericArchiveEntry> GetMockedGenericArchiveEntry(string name, string fullName)
    {
        var mock = new Mock<IGenericArchiveEntry>();
        mock.SetupGet(entry => entry.Name).Returns(name);
        mock.SetupGet(entry => entry.FullName).Returns(fullName);

        return mock;
    }

    private Mock<IOtherArchiveReader> LetArchiveEntriesBe(string archiveFileName, params IGenericArchiveEntry[] entries)
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
            .Setup(file => file.OpenReader(archiveFileName))
            .Returns(readerMock.Object);

        return readerMock;
    }

    private void LetDirectoryNotExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(false);

    private void LetFileExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(path))
            .Returns(true);
}
