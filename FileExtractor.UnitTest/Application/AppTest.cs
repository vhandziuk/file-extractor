using System;
using System.IO;
using System.Threading.Tasks;
using FileExtractor.Application;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils.Compression;
using FileExtractor.Utils.FileSystem;
using Moq;
using Xunit;

namespace FileExtractor.UnitTest.Application;

public class AppTest
{
    private const string SomeSourcePath = @"C:\Source";
    private const string SomeDestinationPath = @"C:\Destination\Extracted";
    private const string SomeConfigurationPath = @"C:\Source\configuration.csv";

    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<ICsvFileInfoProvider> _fileInfoProviderMock = new();
    private readonly Mock<IArchiveExtractor> _archiveExtractorMock = new();
    private readonly Mock<ILogger<App>> _loggerMock = new();
    private readonly ICommandLineOptions _commandlineOptions = Mock.Of<ICommandLineOptions>(options =>
        options.Source == SomeSourcePath
        && options.Destination == SomeDestinationPath
        && options.Configuration == SomeConfigurationPath);

    private readonly App _sut;

    public AppTest()
    {
        _sut = new App(
            _fileSystemUtilsMock.Object,
            _fileInfoProviderMock.Object,
            _archiveExtractorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_SourceDirectoryDoesNotContainZipFiles_DoesNotExtractFiles()
    {
        LetGetFilesReturn(SomeSourcePath, Array.Empty<string>());

        await _sut.RunAsync(_commandlineOptions);

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

        await _sut.RunAsync(_commandlineOptions);

        _loggerMock.Verify(logger =>
            logger.Warning("Supplied configuration contains files to extract. The program will now exit"));
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_SourceDirectoryContainsZipFilesAndFileDataContainsEntries_ExtractsFiles()
    {
        var archives = new[] { "file1.zip", "file2.zip" };
        var fileData = new[] { new FileInfoData("", "file1.zip", string.Empty) };
        LetGetFilesReturn(SomeSourcePath, archives);
        LetEnumerateEntriesReturn(SomeConfigurationPath, fileData);

        await _sut.RunAsync(_commandlineOptions);

        _loggerMock.Verify(logger =>
            logger.Information("Starting file extraction"));
        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFiles(archives, SomeDestinationPath, fileData), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Information("File exctaction completed"));
    }

    [Fact]
    public async Task RunAsync_EnumerateEntriesThrows_DoesNotExtractFiles()
    {
        var expectedException = new IOException();
        LetGetFilesReturn(SomeSourcePath, "file1.zip", "file2.zip");
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(SomeConfigurationPath))
            .Throws(expectedException);

        await _sut.RunAsync(_commandlineOptions);

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

        await _sut.RunAsync(_commandlineOptions);

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "An exception occurred. The program will now exit"));
    }

    private void LetGetFilesReturn(string path, params string[] fileNames) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.GetFiles(path, "*.*", SearchOption.AllDirectories))
            .Returns(fileNames);

    private void LetEnumerateEntriesReturn(string path, params FileInfoData[] entries) =>
        _fileInfoProviderMock
            .Setup(provider => provider.EnumerateEntries(path))
            .Returns(entries);
}