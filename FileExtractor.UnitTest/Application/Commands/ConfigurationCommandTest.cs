using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor.Application.Commands;
using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using FileExtractor.Utils.Compression;
using Moq;
using Xunit;
using static System.Environment;

namespace FileExtractor.UnitTest.Application.Commands;

public class ConfigurationCommandTest
{
    private const string SomeLocalAppDataDirectory = @"C:\Users\SomeUser\AppData\Local";
    private const string SomeSourcePath = @"C:\Source";
    private const string SomeOutputPath = @"C:\Output\Extracted";
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
    private readonly Mock<IExtractionSettingsFactory> _extractionSettingsFactoryMock = new();
    private readonly Mock<IOutputPathResolver> _outputPathResolverMock = new();
    private readonly Mock<IReparsePointGuard> _reparsePointGuardMock = new();
    private readonly Mock<ILogger<ConfigurationCommand>> _loggerMock = new();
    private readonly Mock<IConfigurationOptions> _optionsMock = new();

    private IConfigurationOptions _options => _optionsMock.Object;

    private readonly ConfigurationCommand _sut;

    public ConfigurationCommandTest()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeOutputPath, SomeConfigurationPath, false);

        LetLocalAppDataDirectoryBe(SomeLocalAppDataDirectory);
        LetSettingsFactoryUseProcessorCount(SomeProcessorCount);
        LetDirectoryExist(SomeSourcePath);
        LetFileExist(SomeConfigurationPath);
        _outputPathResolverMock
            .Setup(resolver => resolver.Resolve(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((outputPath, defaultPath) => outputPath ?? defaultPath);

        _sut = new ConfigurationCommand(
            _environmentMock.Object,
            _fileSystemUtilsMock.Object,
            _fileInfoProviderMock.Object,
            _archiveFinderMock.Object,
            _archiveExtractorMock.Object,
            _extractionSettingsFactoryMock.Object,
            _outputPathResolverMock.Object,
            _reparsePointGuardMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_SourcePathDoesNotExist_DoesNotExtractFiles()
    {
        LetDirectoryNotExist(SomeSourcePath);

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("Source path {Path} does not exist. The program will now exit", SomeSourcePath));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
        _outputPathResolverMock.VerifyNoOtherCalls();
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
        SetupCommandLineOptions(SomeSourcePath, SomeOutputPath, configurationPath: null, false);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(SomeConfigurationPath), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ConfiguredFileDoesNotExist_UsesConfigurationFromSourceDirectory()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeOutputPath, SomeOtherConfigurationPath, false);
        LetFileNotExist(SomeOtherConfigurationPath);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileInfoProviderMock.Verify(provider => provider.EnumerateEntries(SomeConfigurationPath), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ConfiguredFileExists_UsesConfiguredFile()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeOutputPath, SomeOtherConfigurationPath, false);
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
        VerifyExtractFilesAsync(new[] { "file1.zip", @"Nestedile2.zip" }, SomeOutputPath, fileData, SomeProcessorCount);
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
        VerifyExtractFilesAsync(archives, SomeOutputPath, fileData, SomeProcessorCount);
        _loggerMock.Verify(logger =>
            logger.Information("File extraction completed"));
        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(SomeConfigurationPath, CachedConfigurationPath, true), cacheConfiguration ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task RunAsync_ArchivesAndFilesFound_ResolvesOutputWithSourceAsDefault()
    {
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _outputPathResolverMock.Verify(resolver => resolver.Resolve(SomeOutputPath, SomeSourcePath), Times.Once);
    }

    [Fact]
    public async Task RunAsync_OutputIsNotProvided_ExtractsFilesToSourcePath()
    {
        var fileData = new[] { new FileInfoData("", "file1.dat", "") };
        SetupCommandLineOptions(SomeSourcePath, outputPath: null, SomeConfigurationPath, false);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await RunAsync();

        VerifyExtractFilesAsync(new[] { "file1.zip" }, SomeSourcePath, fileData, SomeProcessorCount);
    }

    [Fact]
    public async Task RunAsync_ConfigurationContainsNoFiles_DoesNotResolveOutput()
    {
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath);

        await RunAsync();

        _outputPathResolverMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ConfigurationContainsDuplicateRows_PassesDistinctRowsToExtractor()
    {
        var fileInfo1 = new FileInfoData("", "file1.dat", "");
        var fileInfo2 = new FileInfoData("Test", "file2.dat", "SomeDirectory");
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileInfo1, fileInfo2, fileInfo1 with { });

        await RunAsync();

        VerifyExtractFilesAsync(new[] { "file1.zip" }, SomeOutputPath, new[] { fileInfo1, fileInfo2 }, SomeProcessorCount);
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
    public async Task RunAsync_ArchivesAndFilesFound_CreatesSettingsFromOptions()
    {
        var fileData = new[] { new FileInfoData("", "file1.dat", "") };
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await RunAsync();

        _extractionSettingsFactoryMock.Verify(factory =>
            factory.Create(_options, SomeOutputPath, It.Is<IReadOnlyList<FileInfoData>>(files => files.SequenceEqual(fileData))), Times.Once);
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

        await _sut.RunAsync(_options, cancellationTokenSource.Token);

        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<ExtractionSettings>(),
                cancellationTokenSource.Token), Times.Once);
    }

    [Fact]
    public async Task RunAsync_EnumerateEntriesThrows_PropagatesExceptionAndDoesNotExtractFiles()
    {
        var expectedException = new IOException();
        LetArchivesBe(SomeSourcePath, "file1.zip", "file2.zip");
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(SomeConfigurationPath))
            .Throws(expectedException);

        var exception = await Assert.ThrowsAsync<IOException>(RunAsync);

        Assert.Same(expectedException, exception);
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ExtractionCancelled_PropagatesCancellationAndDoesNotCacheConfiguration()
    {
        LetCacheConfigurationBe(true);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, new FileInfoData("", "file1.dat", ""));
        _archiveExtractorMock
            .Setup(extractor => extractor.ExtractFilesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<ExtractionSettings>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(RunAsync);

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
        SetupCommandLineOptions(SomeSourcePath, SomeOutputPath, cachedConfigurationPathInOtherCase, true);
        LetFileExist(cachedConfigurationPathInOtherCase);
        LetArchivesBe(SomeSourcePath, "file1.zip");
        LetEnumerateEntriesReturn(cachedConfigurationPathInOtherCase, new FileInfoData("", "file1.dat", ""));

        await RunAsync();

        _fileSystemUtilsMock.Verify(utils =>
            utils.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    private Task RunAsync() =>
        _sut.RunAsync(_options, TestContext.Current.CancellationToken);

    private void VerifyExtractFilesAsync(string[] archives, string outputPath, FileInfoData[] fileData, int maxDegreeOfParallelism) =>
        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                archives,
                It.Is<ExtractionSettings>(settings =>
                    settings.OutputPath == outputPath
                    && settings.Files.SequenceEqual(fileData)
                    && settings.MaxDegreeOfParallelism == maxDegreeOfParallelism),
                It.IsAny<CancellationToken>()), Times.Once);

    private static IEnumerable<FileInfoData> CountEnumerations(Action onEnumerate, params FileInfoData[] entries)
    {
        onEnumerate();
        foreach (var entry in entries)
            yield return entry;
    }

    private void SetupCommandLineOptions(string sourcePath, string outputPath, string configurationPath, bool cacheConfiguration)
    {
        _optionsMock
            .Setup(options => options.Source)
            .Returns(sourcePath);
        _optionsMock
            .Setup(options => options.Output)
            .Returns(outputPath);
        _optionsMock
            .Setup(options => options.Configuration)
            .Returns(configurationPath);
        _optionsMock
            .Setup(options => options.CacheConfiguration)
            .Returns(cacheConfiguration);
    }

    private void LetLocalAppDataDirectoryBe(string path) =>
        _environmentMock
            .Setup(environment => environment.GetFolderPath(SpecialFolder.LocalApplicationData))
            .Returns(path);

    private void LetSettingsFactoryUseProcessorCount(int processorCount) =>
        _extractionSettingsFactoryMock
            .Setup(factory => factory.Create(It.IsAny<IExtractionOptions>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<FileInfoData>>()))
            .Returns<IExtractionOptions, string, IReadOnlyList<FileInfoData>>((_, outputPath, files) => new ExtractionSettings(outputPath, files, processorCount));

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


    private void LetCacheConfigurationBe(bool cacheConfiguration) =>
        _optionsMock
            .Setup(options => options.CacheConfiguration)
            .Returns(cacheConfiguration);


    private void LetFollowSymbolicLinksBe(bool followSymbolicLinks) =>
        _optionsMock
            .Setup(options => options.FollowSymbolicLinks)
            .Returns(followSymbolicLinks);

    private void LetRecursiveBe(bool recursive) =>
        _optionsMock
            .Setup(options => options.Recursive)
            .Returns(recursive);
}
