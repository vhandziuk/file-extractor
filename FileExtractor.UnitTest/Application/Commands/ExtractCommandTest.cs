using System;
using System.Collections.Generic;
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

namespace FileExtractor.UnitTest.Application.Commands;

public class ExtractCommandTest
{
    private const string SomeArchivePath = @"C:\Source\*.zip";
    private const string SomeArchive = @"C:\Source\SomeArchive.zip";
    private const string SomeOutputPath = @"C:\Output";
    private const string SomeCurrentDirectory = @"C:\Current";
    private const int SomeProcessorCount = 8;

    private static readonly FileInfoData AllFiles = new("", "*", "");

    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly Mock<IFileSystemUtils> _fileSystemUtilsMock = new();
    private readonly Mock<IFilePatternParser> _filePatternParserMock = new();
    private readonly Mock<IArchiveFinder> _archiveFinderMock = new();
    private readonly Mock<IArchiveExtractor> _archiveExtractorMock = new();
    private readonly Mock<IExtractionSettingsFactory> _extractionSettingsFactoryMock = new();
    private readonly Mock<ILogger<ExtractCommand>> _loggerMock = new();
    private readonly Mock<IExtractOptions> _optionsMock = new();

    private readonly ExtractCommand _sut;

    public ExtractCommandTest()
    {
        _optionsMock.SetupGet(options => options.Archive).Returns(SomeArchivePath);
        _optionsMock.SetupGet(options => options.Output).Returns(SomeOutputPath);
        LetFilesBe();
        LetExclusionsBe();
        _environmentMock.SetupGet(environment => environment.CurrentDirectory).Returns(SomeCurrentDirectory);
        _filePatternParserMock
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns<string>(pattern => new FileInfoData("", pattern, ""));
        _extractionSettingsFactoryMock
            .Setup(factory => factory.Create(It.IsAny<IExtractionOptions>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<FileInfoData>>()))
            .Returns<IExtractionOptions, string, IReadOnlyList<FileInfoData>>((_, outputPath, files) => new ExtractionSettings(outputPath, files, SomeProcessorCount));
        LetDirectoryExist(SomeOutputPath);
        LetArchivesBe(SomeArchive);

        _sut = new ExtractCommand(
            _environmentMock.Object,
            _fileSystemUtilsMock.Object,
            _filePatternParserMock.Object,
            _archiveFinderMock.Object,
            _archiveExtractorMock.Object,
            _extractionSettingsFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_NoArchivesFound_DoesNotExtractFiles()
    {
        LetArchivesBe();

        await RunAsync();

        _loggerMock.Verify(logger =>
            logger.Warning("No supported archive files found at {Path}. The program will now exit", SomeArchivePath), Times.Once);
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsync_RecursiveOptionProvided_SearchesArchivesWithIt(bool recursive)
    {
        _optionsMock.SetupGet(options => options.Recursive).Returns(recursive);

        await RunAsync();

        _archiveFinderMock.Verify(finder => finder.FindArchives(SomeArchivePath, recursive), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ArchivesFound_ExtractsThemWithSettingsFromFactory()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await _sut.RunAsync(_optionsMock.Object, cancellationTokenSource.Token);

        _extractionSettingsFactoryMock.Verify(factory =>
            factory.Create(_optionsMock.Object, SomeOutputPath, It.IsAny<IReadOnlyList<FileInfoData>>()), Times.Once);
        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                It.Is<IReadOnlyCollection<string>>(archives => archives.SequenceEqual(new[] { SomeArchive })),
                It.Is<ExtractionSettings>(settings => settings.OutputPath == SomeOutputPath && settings.MaxDegreeOfParallelism == SomeProcessorCount),
                cancellationTokenSource.Token), Times.Once);
        _loggerMock.Verify(logger => logger.Information("File extraction completed"), Times.Once);
    }

    [Fact]
    public async Task RunAsync_NoFilesProvided_ExtractsAllFiles()
    {
        await RunAsync();

        VerifySettings(settings => settings.Files.SequenceEqual(new[] { AllFiles }));
    }

    [Fact]
    public async Task RunAsync_FilesProvided_ExtractsDistinctParsedFiles()
    {
        LetFilesBe("*.txt", "docs/SomeName.dat", "*.txt");
        var parsedFile = new FileInfoData("", "SomeName.dat", "docs");
        _filePatternParserMock.Setup(parser => parser.Parse("docs/SomeName.dat")).Returns(parsedFile);

        await RunAsync();

        VerifySettings(settings => settings.Files.SequenceEqual(new[] { new FileInfoData("", "*.txt", ""), parsedFile }));
    }

    [Fact]
    public async Task RunAsync_ExclusionsProvided_PassesParsedExclusions()
    {
        LetExclusionsBe("*.tmp", "*.log");

        await RunAsync();

        VerifySettings(settings => settings.ExcludedFiles.SequenceEqual(new[] { new FileInfoData("", "*.tmp", ""), new FileInfoData("", "*.log", "") }));
    }

    [Fact]
    public async Task RunAsync_NoExclusionsProvided_PassesNoExclusions()
    {
        await RunAsync();

        VerifySettings(settings => settings.ExcludedFiles.Count == 0);
    }

    [Fact]
    public async Task RunAsync_PatternIsInvalid_ThrowsBeforeSearchingArchives()
    {
        LetFilesBe("docs/");
        _filePatternParserMock.Setup(parser => parser.Parse("docs/")).Throws(new ArgumentException("Invalid pattern"));

        await Assert.ThrowsAsync<ArgumentException>(RunAsync);

        _archiveFinderMock.VerifyNoOtherCalls();
        _archiveExtractorMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task RunAsync_FlatOptionProvided_PreservesEntryPathsOnlyWithoutIt(bool flat, bool expectedPreserveEntryPaths)
    {
        _optionsMock.SetupGet(options => options.Flat).Returns(flat);

        await RunAsync();

        VerifySettings(settings => settings.PreserveEntryPaths == expectedPreserveEntryPaths);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunAsync_ArchiveFolderOptionProvided_PassesIt(bool archiveFolder)
    {
        _optionsMock.SetupGet(options => options.ArchiveFolder).Returns(archiveFolder);

        await RunAsync();

        VerifySettings(settings => settings.UseArchiveFolders == archiveFolder);
    }

    [Fact]
    public async Task RunAsync_OutputIsNotProvided_ExtractsToCurrentDirectory()
    {
        _optionsMock.SetupGet(options => options.Output).Returns((string)null);
        LetDirectoryExist(SomeCurrentDirectory);

        await RunAsync();

        VerifySettings(settings => settings.OutputPath == SomeCurrentDirectory);
        _fileSystemUtilsMock.Verify(utils => utils.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_OutputDoesNotExist_CreatesIt()
    {
        _fileSystemUtilsMock.Setup(utils => utils.DirectoryExists(SomeOutputPath)).Returns(false);

        await RunAsync();

        _fileSystemUtilsMock.Verify(utils => utils.CreateDirectory(SomeOutputPath), Times.Once);
        VerifySettings(settings => settings.OutputPath == SomeOutputPath);
    }

    [Fact]
    public async Task RunAsync_NoArchivesFound_DoesNotCreateOutput()
    {
        LetArchivesBe();
        _fileSystemUtilsMock.Setup(utils => utils.DirectoryExists(SomeOutputPath)).Returns(false);

        await RunAsync();

        _fileSystemUtilsMock.Verify(utils => utils.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    private Task RunAsync() =>
        _sut.RunAsync(_optionsMock.Object, TestContext.Current.CancellationToken);

    private void VerifySettings(Func<ExtractionSettings, bool> predicate) =>
        _archiveExtractorMock.Verify(extractor =>
            extractor.ExtractFilesAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.Is<ExtractionSettings>(settings => predicate(settings)),
                It.IsAny<CancellationToken>()), Times.Once);

    private void LetFilesBe(params string[] patterns) =>
        _optionsMock.SetupGet(options => options.Files).Returns(patterns);

    private void LetExclusionsBe(params string[] patterns) =>
        _optionsMock.SetupGet(options => options.Exclude).Returns(patterns);

    private void LetArchivesBe(params string[] archives) =>
        _archiveFinderMock
            .Setup(finder => finder.FindArchives(SomeArchivePath, It.IsAny<bool>()))
            .Returns(archives);

    private void LetDirectoryExist(string path) =>
        _fileSystemUtilsMock
            .Setup(utils => utils.DirectoryExists(path))
            .Returns(true);
}
