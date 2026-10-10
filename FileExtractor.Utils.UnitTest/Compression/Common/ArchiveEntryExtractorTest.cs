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

    private readonly Mock<IArchiveEntryMatcher> _archiveEntryMatcherMock = new();
    private readonly Mock<IPathValidator> _pathValidatorMock = new();
    private readonly Mock<IReparsePointGuard> _reparsePointGuardMock = new();
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
        _pathValidatorMock
            .Setup(validator => validator.IsValidFileName(It.IsAny<string>()))
            .Returns(true);
        _pathValidatorMock
            .Setup(validator => validator.IsValidRelativePath(It.IsAny<string>()))
            .Returns(true);
        _pathValidatorMock
            .Setup(validator => validator.IsContained(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        LetConfiguredFilesBe();

        _sut = new ArchiveEntryExtractor(
            _archiveEntryMatcherMock.Object,
            _pathValidatorMock.Object,
            _reparsePointGuardMock.Object,
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
        LetEntryMatch(entryMock, fileInfo);

        var result = ExtractEntries(entryMock.Object);

        Assert.Equal([fileInfo], result);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(Path.Combine(SomeOutputPath, "Test")), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Information("Extracting {File} to {Path}", "SomeName.dat", Path.Combine(SomeOutputPath, "Test")), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test", "SomeName.dat"), false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_DirectoryIsEmpty_ExtractsEntryToOutputPath()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);

        ExtractEntries(entryMock.Object);

        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(SomeOutputPath), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName.dat"), false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_EntryDoesNotMatch_DoesNotExtractEntry()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        _archiveEntryMatcherMock.Verify(matcher =>
            matcher.IsMatch(fileInfo, "SomeName.dat", @"SomeDirectory\SomeName.dat"), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_SomeEntriesMatch_ExtractsOnlyMatchingEntries()
    {
        var entry1Mock = GetMockedGenericArchiveEntry("SomeName1.dat", @"SomeDirectory\SomeName1.dat");
        var entry2Mock = GetMockedGenericArchiveEntry("SomeName2.txt", @"SomeDirectory\SomeName2.txt");
        var entry3Mock = GetMockedGenericArchiveEntry("SomeName3.dat", @"SomeDirectory\Nested\SomeName3.dat");
        var fileInfo = new FileInfoData("", "SomeName?.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entry1Mock, fileInfo);
        LetEntryMatch(entry3Mock, fileInfo);

        var result = ExtractEntries(entry1Mock.Object, entry2Mock.Object, entry3Mock.Object);

        Assert.Equal([fileInfo], result);
        entry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName1.dat"), false), Times.Once);
        entry2Mock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        entry3Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName3.dat"), false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_EntryMatchesMultipleDestinations_ExtractsEntryOnceAndCopiesIt()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo1 = new FileInfoData("Test1", "SomeName.dat", "SomeDirectory");
        var fileInfo2 = new FileInfoData("Test2", "*.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo1, fileInfo2);
        LetEntryMatch(entryMock, fileInfo1, fileInfo2);

        ExtractEntries(entryMock.Object);

        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test1", "SomeName.dat"), false), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(
                Path.Combine(SomeOutputPath, "Test1", "SomeName.dat"),
                Path.Combine(SomeOutputPath, "Test2", "SomeName.dat"),
                false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_RowsMatchEntryInSameDirectory_ExtractsEntryOnceAndReportsBothRows()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo1 = new FileInfoData("Test", "SomeName.dat", "SomeDirectory");
        var fileInfo2 = new FileInfoData("Test", "*.dat", "");
        LetConfiguredFilesBe(fileInfo1, fileInfo2);
        LetEntryMatch(entryMock, fileInfo1, fileInfo2);

        var result = ExtractEntries(entryMock.Object);

        Assert.Equivalent(new[] { fileInfo1, fileInfo2 }, result);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test", "SomeName.dat"), false), Times.Once);
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
        var fileInfo1 = new FileInfoData("Test1", "SomeName.dat", "SomeDirectory1");
        var fileInfo2 = new FileInfoData("Test2", "SomeName.dat", "SomeDirectory2");
        LetConfiguredFilesBe(fileInfo1, fileInfo2);
        LetEntryMatch(entry1Mock, fileInfo1);
        LetEntryMatch(entry2Mock, fileInfo2);

        var result = ExtractEntries(entry1Mock.Object, entry2Mock.Object);

        Assert.Equivalent(new[] { fileInfo1, fileInfo2 }, result);
        entry1Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test1", "SomeName.dat"), false), Times.Once);
        entry2Mock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Test2", "SomeName.dat"), false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_FileAlreadyExists_SkipsExtraction()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
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
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetFilePathBeClaimed(Path.Combine(SomeOutputPath, "SomeName.dat"));

        ExtractEntries(entryMock.Object);

        _loggerMock.Verify(logger =>
            logger.Warning("File {File} already exists in {Path}. Skipping extraction", "SomeName.dat", SomeOutputPath), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_EntryNameIsUnsafe_SkipsEntry()
    {
        var entryMock = GetMockedGenericArchiveEntry(@"..\SomeName.dat", @"SomeDirectory/..\SomeName.dat");
        var fileInfo = new FileInfoData("", "*.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetFileNameBeUnsafe(@"..\SomeName.dat");

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        _loggerMock.Verify(logger =>
            logger.Warning("Archive entry {Entry} has an unsafe name. Skipping it", @"SomeDirectory/..\SomeName.dat"), Times.Once);
        VerifyNothingWritten(entryMock);
    }

    [Theory]
    [InlineData("Root/Nested/SomeName.dat", @"Root\Nested")]
    [InlineData(@"Root\Nested\SomeName.dat", @"Root\Nested")]
    [InlineData("./Root//Nested/SomeName.dat", @"Root\Nested")]
    [InlineData("/Root/SomeName.dat", "Root")]
    [InlineData("SomeName.dat", "")]
    public void ExtractEntries_EntryPathsArePreserved_ExtractsEntryToItsArchiveFolder(string fullName, string expectedEntryDirectory)
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", fullName);
        var fileInfo = new FileInfoData("Test", "SomeName.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetEntryPathsBePreserved();
        var expectedDirectory = Path.Combine(SomeOutputPath, "Test", expectedEntryDirectory);

        var result = ExtractEntries(entryMock.Object);

        Assert.Equal([fileInfo], result);
        _pathValidatorMock.Verify(validator => validator.IsValidRelativePath(expectedEntryDirectory), Times.Once);
        _fileSystemUtilsMock.Verify(utils => utils.CreateDirectory(expectedDirectory), Times.Once);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(expectedDirectory, "SomeName.dat"), false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_EntryPathsAreNotPreserved_ExtractsEntryWithoutItsArchiveFolder()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", "Root/Nested/SomeName.dat");
        var fileInfo = new FileInfoData("", "SomeName.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);

        ExtractEntries(entryMock.Object);

        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "SomeName.dat"), false), Times.Once);
    }

    [Fact]
    public void ExtractEntries_PreservedEntryPathIsUnsafe_SkipsEntry()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dll", "Root/../../SomeName.dll");
        var fileInfo = new FileInfoData("", "*.dll", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetEntryPathsBePreserved();
        LetRelativePathBeUnsafe(@"Root\..\..");

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        _loggerMock.Verify(logger =>
            logger.Warning("Archive entry {Entry} has an unsafe path. Skipping it", "Root/../../SomeName.dll"), Times.Once);
        VerifyNothingWritten(entryMock);
    }

    [Fact]
    public void ExtractEntries_EntryDoesNotMatch_DoesNotValidateEntryName()
    {
        var entryMock = GetMockedGenericArchiveEntry("", "SomeDirectory/");
        LetConfiguredFilesBe(new FileInfoData("", "*", ""));

        ExtractEntries(entryMock.Object);

        _pathValidatorMock.Verify(validator =>
            validator.IsValidFileName(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_DestinationIsOutsideOutputPath_SkipsExtraction()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData(@"C:\Windows", "SomeName.dat", "");
        var destinationFilePath = @"C:\Windows\SomeName.dat";
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetPathBeOutsideOutput(destinationFilePath);

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        _loggerMock.Verify(logger =>
            logger.Warning("Destination {Path} is outside the output path {OutputPath}. Skipping extraction", destinationFilePath, SomeOutputPath), Times.Once);
        VerifyNothingWritten(entryMock);
    }

    [Fact]
    public void ExtractEntries_DestinationContainsReparsePoint_SkipsExtraction()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("Test", "SomeName.dat", "");
        var destinationFilePath = Path.Combine(SomeOutputPath, "Test", "SomeName.dat");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetPathContainReparsePoint(destinationFilePath);

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        _loggerMock.Verify(logger =>
            logger.Warning("Destination {Path} contains a symbolic link or junction. Skipping extraction", destinationFilePath), Times.Once);
        VerifyNothingWritten(entryMock);
    }

    [Fact]
    public void ExtractEntries_DestinationContainsReparsePointAndLinksAreFollowed_ExtractsEntry()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData("Test", "SomeName.dat", "");
        var destinationFilePath = Path.Combine(SomeOutputPath, "Test", "SomeName.dat");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetPathContainReparsePoint(destinationFilePath);
        LetSymbolicLinksBeFollowed();

        var result = ExtractEntries(entryMock.Object);

        Assert.Equal([fileInfo], result);
        entryMock.Verify(entry =>
            entry.ExtractToFile(destinationFilePath, false), Times.Once);
        _reparsePointGuardMock.Verify(guard =>
            guard.ContainsReparsePoint(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_LinksAreFollowedAndDestinationIsOutsideOutputPath_SkipsExtraction()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var fileInfo = new FileInfoData(@"C:\Windows", "SomeName.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);
        LetPathBeOutsideOutput(@"C:\Windows\SomeName.dat");
        LetSymbolicLinksBeFollowed();

        var result = ExtractEntries(entryMock.Object);

        Assert.Empty(result);
        VerifyNothingWritten(entryMock);
    }

    [Fact]
    public void ExtractEntries_OneOfTwoDestinationsIsUnsafe_ExtractsToSafeDestinationAndReportsOnlyItsRow()
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        var safeFileInfo = new FileInfoData("Safe", "SomeName.dat", "");
        var unsafeFileInfo = new FileInfoData("Unsafe", "*.dat", "");
        LetConfiguredFilesBe(unsafeFileInfo, safeFileInfo);
        LetEntryMatch(entryMock, unsafeFileInfo, safeFileInfo);
        LetPathContainReparsePoint(Path.Combine(SomeOutputPath, "Unsafe", "SomeName.dat"));

        var result = ExtractEntries(entryMock.Object);

        Assert.Equal([safeFileInfo], result);
        entryMock.Verify(entry =>
            entry.ExtractToFile(Path.Combine(SomeOutputPath, "Safe", "SomeName.dat"), false), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(Path.Combine(SomeOutputPath, "Unsafe")), Times.Never);
    }

    [Theory]
    [InlineData(unchecked((int)0x80070050))]
    [InlineData(unchecked((int)0x800700B7))]
    public void ExtractEntries_FileAppearsBeforeWriting_SkipsExtractionAndKeepsFile(int hResult)
    {
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        entryMock
            .Setup(entry => entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new IOException("The file exists.", hResult));
        var fileInfo = new FileInfoData("", "SomeName.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);

        ExtractEntries(entryMock.Object);

        _loggerMock.Verify(logger =>
            logger.Warning("File {File} already exists in {Path}. Skipping extraction", "SomeName.dat", SomeOutputPath), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ExtractEntries_ExtractionFails_DeletesPartialFileAndRethrows()
    {
        var expectedException = new IOException();
        var entryMock = GetMockedGenericArchiveEntry("SomeName.dat", @"SomeDirectory\SomeName.dat");
        entryMock
            .Setup(entry => entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(expectedException);
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);

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
        var fileInfo1 = new FileInfoData("Test1", "SomeName.dat", "SomeDirectory");
        var fileInfo2 = new FileInfoData("Test2", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo1, fileInfo2);
        LetEntryMatch(entryMock, fileInfo1, fileInfo2);
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
        var fileInfo = new FileInfoData("", "SomeName.dat", "SomeDirectory");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entryMock, fileInfo);

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
        var fileInfo = new FileInfoData("", "*.dat", "");
        LetConfiguredFilesBe(fileInfo);
        LetEntryMatch(entry1Mock, fileInfo);
        LetEntryMatch(entry2Mock, fileInfo);

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

    private void LetEntryMatch(Mock<IGenericArchiveEntry> entryMock, params FileInfoData[] files)
    {
        var entry = entryMock.Object;
        foreach (var fileInfo in files)
        {
            _archiveEntryMatcherMock
                .Setup(matcher => matcher.IsMatch(fileInfo, entry.Name, entry.FullName))
                .Returns(true);
        }
    }

    private void LetFilePathBeClaimed(string path) =>
        _contextMock
            .Setup(context => context.TryClaimFilePath(path))
            .Returns(false);

    private void LetFileExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(path))
            .Returns(true);

    private void LetFileNameBeUnsafe(string fileName) =>
        _pathValidatorMock
            .Setup(validator => validator.IsValidFileName(fileName))
            .Returns(false);

    private void LetPathBeOutsideOutput(string path) =>
        _pathValidatorMock
            .Setup(validator => validator.IsContained(SomeOutputPath, path))
            .Returns(false);

    private void LetPathContainReparsePoint(string path) =>
        _reparsePointGuardMock
            .Setup(guard => guard.ContainsReparsePoint(SomeOutputPath, path))
            .Returns(true);

    private void LetRelativePathBeUnsafe(string path) =>
        _pathValidatorMock
            .Setup(validator => validator.IsValidRelativePath(path))
            .Returns(false);

    private void LetSymbolicLinksBeFollowed() =>
        _contextMock
            .SetupGet(context => context.FollowSymbolicLinks)
            .Returns(true);

    private void LetEntryPathsBePreserved() =>
        _contextMock
            .SetupGet(context => context.PreserveEntryPaths)
            .Returns(true);

    private void VerifyNothingWritten(Mock<IGenericArchiveEntry> entryMock)
    {
        entryMock.Verify(entry =>
            entry.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(It.IsAny<string>()), Times.Never);
    }
}
