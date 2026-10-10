using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.Compression.Common;
using Moq;
using Xunit;

namespace FileExtractor.Utils.UnitTest.Compression.Common;

public class ArchiveEntryExtractorTest
{
    private const string SomeArchiveFileName = @"C:\SomeArchive.zip";
    private const string SomeOutputPath = @"C:\Output";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IExtractionContext> _contextMock = new();
    private readonly Mock<ILogger<ArchiveEntryExtractor>> _loggerMock = new();

    private readonly ArchiveEntryExtractor _sut;

    public ArchiveEntryExtractorTest()
    {
        _contextMock
            .SetupGet(context => context.OutputPath)
            .Returns(SomeOutputPath);
        _contextMock
            .Setup(context => context.TryClaimFilePath(It.IsAny<string>()))
            .Returns(true);
        LetConfiguredFilesBe();

        _sut = new ArchiveEntryExtractor(
            _fileSystemUtilsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void ExtractEntries_ArchiveContainsNoEntries_LogsWarning()
    {
        LetConfiguredFilesBe(new FileInfoData("", "SomeName.dat", ""));

        var result = ExtractEntries();

        Assert.Empty(result);
        _loggerMock.Verify(logger =>
            logger.Warning("Archive {Archive} contains no entries", SomeArchiveFileName), Times.Once);
    }

    [Fact]
    public void ExtractEntries_EntryMatches_ExtractsEntryToDestinationDirectory()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("Test", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);

        var result = ExtractEntries(entryMock.Object);

        Assert.Equal([fileInfo], result);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(Path.Combine(SomeOutputPath, "Test")), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Information("Extracting {File} to {Path}", "SomeName.dat", Path.Combine(SomeOutputPath, "Test")), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test", "SomeName.dat"), true), Times.Once);
    }

    [Fact]
    public void ExtractEntries_DirectoryIsEmpty_ExtractsEntryToOutputPath()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        LetConfiguredFilesBe(new FileInfoData("", "SomeName.dat", "SomeDirectory"));

        ExtractEntries(entryMock.Object);

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeOutputPath), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName.dat"), true), Times.Once);
    }

    [Theory]
    [InlineData("SomeOtherName.dat", @"SomeDirectory\SomeOtherName.dat")]
    [InlineData("SomeName.dat", @"SomeOtherDirectory\SomeName.dat")]
    public void ExtractEntries_EntryDoesNotMatch_DoesNotExtractEntry(string entryName, string entryFullName)
    {
        var entryMock = GetMockedGenericArchiveEntry(entryName, entryFullName);
        LetConfiguredFilesBe(new FileInfoData("", "SomeName.dat", "SomeDirectory"));

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_FileNameContainsWildcards_ExtractsMatchingEntries()
    {
        var entry1Mock = GetMockedGenericArchiveEntry("SomeName1.dat", @"SomeDirectory\SomeName1.dat");
        var entry2Mock = GetMockedGenericArchiveEntry("SomeName2.dat", @"SomeDirectory\Nested\SomeName2.dat");
        var entry3Mock = GetMockedGenericArchiveEntry("SomeName3.txt", @"SomeDirectory\SomeName3.txt");
        var fileInfo = new FileInfoData("", "SomeName?.dat", "");
        LetConfiguredFilesBe(fileInfo);

        var result = ExtractEntries(entry1Mock.Object, entry2Mock.Object, entry3Mock.Object);

        Assert.Equal([fileInfo], result);
        entry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName1.dat"), true), Times.Once);
        entry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName2.dat"), true), Times.Once);
        entry3Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_EntryMatchesMultipleDestinations_ExtractsEntryOnceAndCopiesIt()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        LetConfiguredFilesBe(
            new FileInfoData("Test1", "SomeName.dat", "SomeDirectory"),
            new FileInfoData("Test2", "*.dat", "SomeDirectory"));

        ExtractEntries(entryMock.Object);

        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test1", "SomeName.dat"), true), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(
                Path.Combine(SomeOutputPath, "Test1", "SomeName.dat"),
                Path.Combine(SomeOutputPath, "Test2", "SomeName.dat"),
                true), Times.Once);
    }

    [Fact]
    public void ExtractEntries_RowsMatchEntryInSameDirectory_ExtractsEntryOnceAndReportsBothRows()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo1 = new FileInfoData("Test", "SomeName.dat", "SomeDirectory");
        var fileInfo2 = new FileInfoData("Test", "*.dat", "");
        LetConfiguredFilesBe(fileInfo1, fileInfo2);

        var result = ExtractEntries(entryMock.Object);

        Assert.Equivalent(new[] { fileInfo1, fileInfo2 }, result);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test", "SomeName.dat"), true), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _loggerMock.Verify(logger =>
            logger.Warning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_SameNameConfiguredForDifferentLocations_ExtractsEachEntry()
    {
        var entry1Mock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory1\SomeName.dat");
        var entry2Mock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory2\SomeName.dat");
        LetConfiguredFilesBe(
            new FileInfoData("Test1", "SomeName.dat", "SomeDirectory1"),
            new FileInfoData("Test2", "SomeName.dat", "SomeDirectory2"));

        var result = ExtractEntries(entry1Mock.Object, entry2Mock.Object);

        Assert.Equal(2, result.Count);
        entry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test1", "SomeName.dat"), true), Times.Once);
        entry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test2", "SomeName.dat"), true), Times.Once);
    }

    [Fact]
    public void ExtractEntries_FileAlreadyExists_SkipsExtraction()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);
        LetFileExist(Path.Combine(SomeOutputPath, "SomeName.dat"));

        var result = ExtractEntries(entryMock.Object);

        Assert.Equal([fileInfo], result);
        _loggerMock.Verify(logger =>
            logger.Warning("File {File} already exists in {Path}. Skipping extraction", "SomeName.dat", SomeOutputPath), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_FilePathAlreadyClaimed_SkipsExtraction()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        LetConfiguredFilesBe(new FileInfoData("", "SomeName.dat", "SomeDirectory"));
        LetFilePathBeClaimed(Path.Combine(SomeOutputPath, "SomeName.dat"));

        ExtractEntries(entryMock.Object);

        _loggerMock.Verify(logger =>
            logger.Warning("File {File} already exists in {Path}. Skipping extraction", "SomeName.dat", SomeOutputPath), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_ExtractionFails_DeletesPartialFileAndRethrows()
    {
        var expectedException = new IOException();
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        entryMock
            .Setup(entry => entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(expectedException);
        LetConfiguredFilesBe(new FileInfoData("", "SomeName.dat", "SomeDirectory"));

        var exception = Assert.Throws<IOException>(() => ExtractEntries(entryMock.Object));

        Assert.Same(expectedException, exception);
        _fileSystemUtilsMock.Verify(utils =>
            utils.DeleteFile(Path.Combine(SomeOutputPath, "SomeName.dat")), Times.Once);
    }

    [Fact]
    public void ExtractEntries_CopyFails_DeletesPartialCopyAndRethrows()
    {
        var expectedException = new IOException();
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        LetConfiguredFilesBe(
            new FileInfoData("Test1", "SomeName.dat", "SomeDirectory"),
            new FileInfoData("Test2", "SomeName.dat", "SomeDirectory"));
        _fileSystemUtilsMock
            .Setup(utils => utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(expectedException);

        var exception = Assert.Throws<IOException>(() => ExtractEntries(entryMock.Object));

        Assert.Same(expectedException, exception);
        _fileSystemUtilsMock.Verify(utils =>
            utils.DeleteFile(Path.Combine(SomeOutputPath, "Test2", "SomeName.dat")), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.DeleteFile(Path.Combine(SomeOutputPath, "Test1", "SomeName.dat")), Times.Never);
    }

    [Fact]
    public void ExtractEntries_DeletingPartialFileFails_LogsErrorAndRethrowsOriginalException()
    {
        var expectedException = new IOException();
        var deleteException = new UnauthorizedAccessException();
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        entryMock
            .Setup(entry => entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(expectedException);
        _fileSystemUtilsMock
            .Setup(utils => utils.DeleteFile(It.IsAny<string>()))
            .Throws(deleteException);
        LetConfiguredFilesBe(new FileInfoData("", "SomeName.dat", "SomeDirectory"));

        var exception = Assert.Throws<IOException>(() => ExtractEntries(entryMock.Object));

        Assert.Same(expectedException, exception);
        _loggerMock.Verify(logger =>
            logger.Error(deleteException, "Failed to delete partially extracted file {File}", Path.Combine(SomeOutputPath, "SomeName.dat")), Times.Once);
    }

    [Fact]
    public void ExtractEntries_CancellationRequested_StopsBeforeNextEntry()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var entry1Mock = GetMockedGenericArchiveEntry("SomeName1.dat", @"SomeDirectory\SomeName1.dat");
        var entry2Mock = GetMockedGenericArchiveEntry("SomeName2.dat", @"SomeDirectory\SomeName2.dat");
        entry1Mock
            .Setup(entry => entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()))
            .Callback(cancellationTokenSource.Cancel);
        LetConfiguredFilesBe(new FileInfoData("", "*.dat", ""));

        Assert.Throws<OperationCanceledException>(() =>
            _sut.ExtractEntries(SomeArchiveFileName, [entry1Mock.Object, entry2Mock.Object], _contextMock.Object, cancellationTokenSource.Token));

        entry1Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        entry2Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    private IReadOnlyCollection<FileInfoData> ExtractEntries(params IGenericArchiveEntry[] entries) =>
        _sut.ExtractEntries(SomeArchiveFileName, entries, _contextMock.Object, TestContext.Current.CancellationToken);

    private static Mock<IGenericArchiveEntry> GetMockedGenericArchiveEntry(string name, string fullName)
    {
        var mock = new Mock<IGenericArchiveEntry>();
        mock.SetupGet(entry => entry.Name).Returns(name);
        mock.SetupGet(entry => entry.FullName).Returns(fullName);

        return mock;
    }

    private void LetConfiguredFilesBe(params FileInfoData[] files) =>
        _contextMock
            .SetupGet(context => context.Files)
            .Returns(files);

    private void LetFilePathBeClaimed(string path) =>
        _contextMock
            .Setup(context => context.TryClaimFilePath(path))
            .Returns(false);

    private void LetFileExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(path))
            .Returns(true);
}
