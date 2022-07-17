using System;
using System.IO;
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
    private const string SomeCommonAppDataDirectory = @"C:\ProgramData";
    private const string SomeAppBaseDirectory = @"C:\Program Files\File Extractor";
    private const string SomeSourcePath = @"C:\Source";
    private const string SomeDestinationPath = @"C:\Destination\Extracted";
    private const string SomeConfigurationPath = @"C:\Source\configuration.csv";

    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<ICsvFileInfoProvider> _fileInfoProviderMock = new();
    private readonly Mock<IArchiveExtractor> _archiveExtractorMock = new();
    private readonly Mock<ILogger<App>> _loggerMock = new();
    private readonly Mock<ICommandLineOptions> _commandLineOptionsMock = new();

    private ICommandLineOptions _commandLineOptions => _commandLineOptionsMock.Object;

    private readonly App _sut;

    public AppTest()
    {
        SetupCommandLineOptions(SomeSourcePath, SomeDestinationPath, SomeConfigurationPath, false);

        LetCommonAppDataDirectoryBe(SomeCommonAppDataDirectory);
        LetAppBaseDirectoryBe(SomeAppBaseDirectory);
        LetDirectoryExist(SomeSourcePath);
        LetDirectoryExist(SomeDestinationPath);
        LetFileExist(SomeConfigurationPath);

        _sut = new App(
            _environmentMock.Object,
            _fileSystemUtilsMock.Object,
            _fileInfoProviderMock.Object,
            _archiveExtractorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_UnableToLocateConfigurationFile_DoesNotExtractFiles()
    {
        LetFileNotExist(SomeConfigurationPath);
        LetFileNotExist(Path.Combine(SomeSourcePath, "configuration.csv"));
        LetFileNotExist(Path.Combine(SomeCommonAppDataDirectory, "File Extractor", "configuration.csv"));

        await _sut.RunAsync(_commandLineOptions);

        _loggerMock.Verify(logger =>
            logger.Warning("Unable to locate the configuration file. The program will now exit"));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_SourceDirectoryDoesNotContainZipFiles_DoesNotExtractFiles()
    {
        LetGetFilesReturn(SomeSourcePath, Array.Empty<string>());

        await _sut.RunAsync(_commandLineOptions);

        _loggerMock.Verify(logger =>
            logger.Warning("Source directory contains no supported archive files. The program will now exit"));
        _fileInfoProviderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_FileDataDoesNotContainAnyEntries_DoesNotExtractFiles()
    {
        LetGetFilesReturn(SomeSourcePath, "file1.zip", "file2.zip");
        LetEnumerateEntriesReturn(SomeConfigurationPath, Array.Empty<FileInfoData>());

        await _sut.RunAsync(_commandLineOptions);

        _loggerMock.Verify(logger =>
            logger.Warning("Supplied configuration contains no files to extract. The program will now exit"));
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunAsync_SourceDirectoryContainsZipFilesAndFileDataContainsEntries_ExtractsFiles(bool cacheConfiguration)
    {
        var archives = new[] { "file1.zip", "file2.zip" };
        var fileData = new[] { new FileInfoData("", "file1.zip", string.Empty) };
        LetGetFilesReturn(SomeSourcePath, archives);
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);
        LetCacheConfiguratinBe(cacheConfiguration);

        await _sut.RunAsync(_commandLineOptions);

        _loggerMock.Verify(logger =>
            logger.Information("Starting file extraction"));
        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFiles(archives, SomeDestinationPath, fileData), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Information("File extraction completed"));
        _fileSystemUtilsMock.Verify(utils => utils.Copy(SomeConfigurationPath, Path.Combine(SomeCommonAppDataDirectory, "File Extractor", "configuration.csv"), true), cacheConfiguration ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task RunAsync_EnumerateEntriesThrows_DoesNotExtractFiles()
    {
        var expectedException = new IOException();
        LetGetFilesReturn(SomeSourcePath, "file1.zip", "file2.zip");
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(SomeConfigurationPath))
            .Throws(expectedException);

        await _sut.RunAsync(_commandLineOptions);

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "An exception occurred. The program will now exit"));
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ExtractFilesThrows_LogsException()
    {
        var expectedException = new Exception();
        var archives = new[] { "file1.zip", "file2.zip" };
        var fileData = new[] { new FileInfoData("", "file1.zip", string.Empty) };
        LetGetFilesReturn(SomeSourcePath, archives);
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);
        _archiveExtractorMock
            .Setup(extractor => extractor.ExtractFiles(archives, SomeDestinationPath, fileData))
            .ThrowsAsync(expectedException);

        await _sut.RunAsync(_commandLineOptions);

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "An exception occurred. The program will now exit"));
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

    private void LetCommonAppDataDirectoryBe(string path) =>
        _environmentMock
            .Setup(environment => environment.GetFolderPath(SpecialFolder.CommonApplicationData))
            .Returns(path);

    private void LetAppBaseDirectoryBe(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.GetAppBaseDirectory())
            .Returns(path);

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);

    private void LetFileExist(string filePath) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(filePath))
            .Returns(true);

    private void LetFileNotExist(string filePath) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.FileExists(filePath))
            .Returns(false);

    private void LetGetFilesReturn(string path, params string[] fileNames) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.GetFiles(path, "*.*", SearchOption.AllDirectories))
            .Returns(fileNames);

    private void LetEnumerateEntriesReturn(string path, params FileInfoData[] entries) =>
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(path))
            .Returns(entries);

    private void LetCacheConfiguratinBe(bool cacheConfiguration) =>
        _commandLineOptionsMock
            .Setup(options => options.CacheConfiguration)
            .Returns(cacheConfiguration);
}