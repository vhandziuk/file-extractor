using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor.Application;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;
using Moq;
using Xunit;
using static System.Environment;

namespace FileExtractor.UnitTest.Application;

public class AppTest
{
    private const string SomeLocalAppDataDirectory = @"C:\Users\SomeUser\AppData\Local";
    private const string SomeSourcePath = @"C:\Source";
    private const string SomeDestinationPath = @"C:\Destination\Extracted";
    private const string SomeConfigurationPath = @"C:\Source\configuration.csv";
    private const string SomeOtherConfigurationPath = @"C:\Config\custom.csv";
    private const int SomeProcessorCount = 8;

    private static readonly string CachedConfigurationDirectory = Path.Combine(SomeLocalAppDataDirectory, "File Extractor");
    private static readonly string CachedConfigurationPath = Path.Combine(CachedConfigurationDirectory, "configuration.csv");

    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<ICsvFileInfoProvider> _fileInfoProviderMock = new();
    private readonly Mock<IArchiveFinder> _archiveFinderMock = new();
    private readonly Mock<IArchiveExtractor> _archiveExtractorMock = new();
    private readonly Mock<IReparsePointGuard> _reparsePointGuardMock = new();
    private readonly Mock<ILogger<App>> _loggerMock = new();
    private readonly Mock<ICommandLineOptions> _commandLineOptionsMock = new();

    private ICommandLineOptions _commandLineOptions => _commandLineOptionsMock.Object;

    private readonly App _sut;

    public AppTest()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeDestinationPath, SomeConfigurationPath, false);

        LetLocalAppDataDirectoryBe(SomeLocalAppDataDirectory);
        LetProcessorCountBe(SomeProcessorCount);
        LetDirectoryExist(SomeSourcePath);
        LetDirectoryExist(SomeDestinationPath);
        LetFileExist(SomeConfigurationPath);

        _sut = new App(
            _environmentMock.Object,
            _fileSystemUtilsMock.Object,
            _fileInfoProviderMock.Object,
            _archiveFinderMock.Object,
            _archiveExtractorMock.Object,
            _reparsePointGuardMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_SourcePathIsNotProvided_DoesNotExtractFiles()
    {
        SetupCommandLineOptions(sourcePath: null, SomeDestinationPath, SomeConfigurationPath, false);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Source path is not provided or does not exist. The program will now exit"));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_SourcePathDoesNotExist_DoesNotExtractFiles()
    {
        LetDirectoryNotExist(SomeSourcePath);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Source path is not provided or does not exist. The program will now exit"));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_UnableToLocateConfigurationFile_DoesNotExtractFiles()
    {
        LetFileNotExist(SomeConfigurationPath);
        LetFileNotExist(Path.Combine(SomeSourcePath, "configuration.csv"));
        LetFileNotExist(CachedConfigurationPath);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Unable to locate the configuration file. The program will now exit"));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ConfigurationOptionIsNotProvided_UsesConfigurationFromSourceDirectory()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeDestinationPath, configurationPath: null, false);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(SomeConfigurationPath), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ConfiguredFileDoesNotExist_UsesConfigurationFromSourceDirectory()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeDestinationPath, SomeOtherConfigurationPath, false);
        LetFileNotExist(SomeOtherConfigurationPath);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(SomeConfigurationPath), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ConfiguredFileExists_UsesConfiguredFile()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeDestinationPath, SomeOtherConfigurationPath, false);
        LetFileExist(SomeOtherConfigurationPath);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeOtherConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(SomeOtherConfigurationPath), Times.Once);
        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(SomeConfigurationPath), Times.Never);
    }

    [Fact]
    public async Task RunAsync_OnlyCachedConfigurationExists_UsesCachedConfiguration()
    {
        LetFileNotExist(SomeConfigurationPath);
        LetFileExist(CachedConfigurationPath);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(CachedConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(CachedConfigurationPath), Times.Once);
    }

    [Fact]
    public async Task RunAsync_SourceDirectoryDoesNotContainArchives_DoesNotExtractFiles()
    {
        LetArchivesBe(SomeSourcePath);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Source directory contains no supported archive files. The program will now exit"));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsync_RecursiveOptionProvided_SearchesArchivesWithMatchingScope(bool recursive)
    {
        var fileData = new[] { new FileInfoData("", "file1.dat", "") };
        LetRecursiveBe(recursive);
        _archiveFinderMock
            .Setup(finder => finder.FindArchives(SomeSourcePath, recursive))
            .Returns(["file1.zip", @"Nestedile2.zip"]);
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await RunAsync();

        _archiveFinderMock.Verify(finder => finder.FindArchives(SomeSourcePath, recursive), Times.Once);
        VerifyExtractFilesAsync(new[] { "file1.zip", @"Nestedile2.zip" }, SomeDestinationPath, fileData, SomeProcessorCount);
    }

    [Fact]
    public async Task RunAsync_FileDataDoesNotContainAnyEntries_DoesNotExtractFiles()
    {
        LetArchivesBe(SomeSourcePath, "file1.zip", "file2.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, Array.Empty<FileInfoData>());

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Supplied configuration contains no files to extract. The program will now exit"));
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunAsync_SourceDirectoryContainsArchivesAndFileDataContainsEntries_ExtractsFiles(bool cacheConfiguration)
    {
        var archives = new[] { "file1.zip", "file2.zip" };
        var fileData = new[] { new FileInfoData("", "file1.zip", string.Empty) };
        LetArchivesBe(SomeSourcePath, archives);
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);
        LetCacheConfigurationBe(cacheConfiguration);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Information("Starting file extraction"));
        VerifyExtractFilesAsync(archives, SomeDestinationPath, fileData, SomeProcessorCount);
        _loggerMock.Verify(logger =>
            logger.Information("File extraction completed"));
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(SomeConfigurationPath, CachedConfigurationPath, true), cacheConfiguration ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task RunAsync_DestinationDoesNotExist_ExtractsFilesToSourcePath()
    {
        var fileData = new[] { new FileInfoData("", "file1.dat", "") };
        LetDirectoryNotExist(SomeDestinationPath);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await RunAsync();

        VerifyExtractFilesAsync(new[] { "file1.zip" }, SomeSourcePath, fileData, SomeProcessorCount);
    }

    [Fact]
    public async Task RunAsync_ConfigurationContainsDuplicateRows_PassesDistinctRowsToExtractor()
    {
        var fileInfo1 = new FileInfoData("", "file1.dat", "");
        var fileInfo2 = new FileInfoData("Test", "file2.dat", "SomeDirectory");
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileInfo1, fileInfo2, fileInfo1 with { });

        await RunAsync();

        VerifyExtractFilesAsync(new[] { "file1.zip" }, SomeDestinationPath, new[] { fileInfo1, fileInfo2 }, SomeProcessorCount);
    }

    [Fact]
    public async Task RunAsync_ConfigurationIsRead_EnumeratesConfigurationOnce()
    {
        var enumerationCount = 0;
        LetArchivesBe(SomeSourcePath, "file1.zip");
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(SomeConfigurationPath))
            .Returns(() => CountEnumerations(() => enumerationCount++, new FileInfoData("", "file1.dat", "")));

        await RunAsync();

        Assert.Equal(1, enumerationCount);
    }

    [Fact]
    public async Task RunAsync_MaxParallelismIsPositive_PassesItToExtractor()
    {
        var fileData = new[] { new FileInfoData("", "file1.dat", "") };
        LetMaxParallelismBe(2);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await RunAsync();

        VerifyExtractFilesAsync(new[] { "file1.zip" }, SomeDestinationPath, fileData, 2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RunAsync_MaxParallelismIsNotPositive_UsesProcessorCount(int? maxParallelism)
    {
        var fileData = new[] { new FileInfoData("", "file1.dat", "") };
        LetMaxParallelismBe(maxParallelism);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await RunAsync();

        VerifyExtractFilesAsync(new[] { "file1.zip" }, SomeDestinationPath, fileData, SomeProcessorCount);
        _loggerMock.Verify(logger =>
            logger.Warning("Maximum parallelism must be a positive number. Using {ProcessorCount} instead", SomeProcessorCount),
            maxParallelism is null ? Times.Never() : Times.Once());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsync_FollowSymbolicLinksOptionProvided_PassesItToExtractor(bool followSymbolicLinks)
    {
        LetFollowSymbolicLinksBe(followSymbolicLinks);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.Is<ExtractionSettings>(settings => settings.FollowSymbolicLinks == followSymbolicLinks),
                It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Warning("Symbolic links and junctions below the destination path are followed"),
            followSymbolicLinks ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task RunAsync_FollowSymbolicLinksOptionProvided_StillProtectsCachedConfiguration()
    {
        LetFollowSymbolicLinksBe(true);
        LetCacheConfigurationBe(true);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));
        _reparsePointGuardMock
            .Setup(guard => guard.ContainsReparsePoint(SomeLocalAppDataDirectory, CachedConfigurationPath))
            .Returns(true);

        await RunAsync();

        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_CancellationTokenProvided_PassesItToExtractor()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await _sut.RunAsync(_commandLineOptions, cancellationTokenSource.Token);

        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<ExtractionSettings>(),
                cancellationTokenSource.Token), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ExtractionCancelled_LogsWarningAndDoesNotCacheConfiguration()
    {
        LetCacheConfigurationBe(true);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));
        LetExtractFilesAsyncThrow(new OperationCanceledException());

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Extraction was cancelled. The program will now exit"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_CacheConfigurationRequested_CreatesCacheDirectoryBeforeCopying()
    {
        var calls = new List<string>();
        LetCacheConfigurationBe(true);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));
        _fileSystemUtilsMock
            .Setup(utils => utils.CreateDirectory(CachedConfigurationDirectory))
            .Callback(() => calls.Add(nameof(IFileSystemUtils.CreateDirectory)));
        _fileSystemUtilsMock
            .Setup(utils => utils.Copy(SomeConfigurationPath, CachedConfigurationPath, true))
            .Callback(() => calls.Add(nameof(IFileSystemUtils.Copy)));

        await RunAsync();

        Assert.Equal(new[] { nameof(IFileSystemUtils.CreateDirectory), nameof(IFileSystemUtils.Copy) }, calls);
    }

    [Fact]
    public async Task RunAsync_CachedConfigurationPathContainsReparsePoint_DoesNotCacheConfiguration()
    {
        LetCacheConfigurationBe(true);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));
        _reparsePointGuardMock
            .Setup(guard => guard.ContainsReparsePoint(SomeLocalAppDataDirectory, CachedConfigurationPath))
            .Returns(true);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Cached configuration path {Path} contains a symbolic link or junction. The configuration is not cached", CachedConfigurationPath), Times.Once);
        _fileSystemUtilsMock.Verify(utils =>
            utils.CreateDirectory(It.IsAny<string>()), Times.Never);
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_UsedConfigurationIsCachedConfiguration_DoesNotCopyConfiguration()
    {
        var cachedConfigurationPathInOtherCase = CachedConfigurationPath.ToUpperInvariant();
        SetupCommandLineOptions(SomeSourcePath, SomeDestinationPath, cachedConfigurationPathInOtherCase, true);
        LetFileExist(cachedConfigurationPathInOtherCase);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(cachedConfigurationPathInOtherCase, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_EnumerateEntriesThrows_DoesNotExtractFiles()
    {
        var expectedException = new IOException();
        LetArchivesBe(SomeSourcePath, "file1.zip", "file2.zip");
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(SomeConfigurationPath))
            .Throws(expectedException);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "An exception occurred. The program will now exit"));
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ExtractFilesThrows_LogsException()
    {
        var expectedException = new Exception();
        LetArchivesBe(SomeSourcePath, "file1.zip", "file2.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.zip", string.Empty));
        LetExtractFilesAsyncThrow(expectedException);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "An exception occurred. The program will now exit"));
    }

    private ValueTask RunAsync() =>
        _sut.RunAsync(_commandLineOptions, TestContext.Current.CancellationToken);

    private void VerifyExtractFilesAsync(string[] archives, string destinationPath, FileInfoData[] fileData, int maxDegreeOfParallelism) =>
        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                archives,
                It.Is<ExtractionSettings>(settings =>
                    settings.OutputPath == destinationPath
                    && settings.Files.SequenceEqual(fileData)
                    && settings.MaxDegreeOfParallelism == maxDegreeOfParallelism),
                It.IsAny<CancellationToken>()), Times.Once);

    private static IEnumerable<FileInfoData> CountEnumerations(Action onEnumerate, params FileInfoData[] entries)
    {
        onEnumerate();
        foreach (var entry in entries)
            yield return entry;
    }

    private void SetupCommandLineOptions(string sourcePath, string destinationPath, string configurationPath, bool cacheConfiguration)
    {
        _commandLineOptionsMock
            .Setup(options => options.Source)
            .Returns(sourcePath);
        _commandLineOptionsMock
            .Setup(options => options.Destination)
            .Returns(destinationPath);
        _commandLineOptionsMock
            .Setup(options => options.Configuration)
            .Returns(configurationPath);
        _commandLineOptionsMock
            .Setup(options => options.CacheConfiguration)
            .Returns(cacheConfiguration);
    }

    private void LetLocalAppDataDirectoryBe(string path) =>
        _environmentMock
            .Setup(environment => environment.GetFolderPath(SpecialFolder.LocalApplicationData))
            .Returns(path);

    private void LetProcessorCountBe(int processorCount) =>
        _environmentMock
            .SetupGet(environment => environment.ProcessorCount)
            .Returns(processorCount);

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);

    private void LetDirectoryNotExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(false);

    private void LetFileExist(string filePath) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(filePath))
            .Returns(true);

    private void LetFileNotExist(string filePath) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(filePath))
            .Returns(false);

    private void LetArchivesBe(string path, params string[] archives) =>
        _archiveFinderMock
            .Setup(finder => finder.FindArchives(path, It.IsAny<bool>()))
            .Returns(archives);

    private void LetEnumerateEntriesReturn(string path, params FileInfoData[] entries) =>
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(path))
            .Returns(entries);

    private void LetExtractFilesAsyncThrow(Exception exception) =>
        _archiveExtractorMock
            .Setup(extractor => extractor.ExtractFilesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<ExtractionSettings>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void LetCacheConfigurationBe(bool cacheConfiguration) =>
        _commandLineOptionsMock
            .Setup(options => options.CacheConfiguration)
            .Returns(cacheConfiguration);

    private void LetMaxParallelismBe(int? maxParallelism) =>
        _commandLineOptionsMock
            .Setup(options => options.MaxParallelism)
            .Returns(maxParallelism);

    private void LetFollowSymbolicLinksBe(bool followSymbolicLinks) =>
        _commandLineOptionsMock
            .Setup(options => options.FollowSymbolicLinks)
            .Returns(followSymbolicLinks);

    private void LetRecursiveBe(bool recursive) =>
        _commandLineOptionsMock
            .Setup(options => options.Recursive)
            .Returns(recursive);
}
