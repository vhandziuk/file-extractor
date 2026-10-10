using FileExtractor.Application.Commands;
using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using FileExtractor.Data;
using FileExtractor.Utils;
using Moq;
using Xunit;

namespace FileExtractor.UnitTest.Application.Commands;

public class ExtractionSettingsFactoryTest
{
    private const string SomeOutputPath = @"C:\Output";
    private const int SomeProcessorCount = 8;

    private static readonly FileInfoData[] SomeFiles = [new("", "SomeName.dat", "")];

    private readonly Mock<IEnvironment> _environmentMock = new();
    private readonly Mock<ILogger<ExtractionSettingsFactory>> _loggerMock = new();
    private readonly Mock<IExtractionOptions> _optionsMock = new();

    private readonly ExtractionSettingsFactory _sut;

    public ExtractionSettingsFactoryTest()
    {
        _environmentMock
            .SetupGet(environment => environment.ProcessorCount)
            .Returns(SomeProcessorCount);

        _sut = new ExtractionSettingsFactory(_environmentMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Create_OutputPathAndFilesProvided_UsesThem()
    {
        var result = _sut.Create(_optionsMock.Object, SomeOutputPath, SomeFiles);

        Assert.Equal(SomeOutputPath, result.OutputPath);
        Assert.Same(SomeFiles, result.Files);
    }

    [Fact]
    public void Create_MaxParallelismIsPositive_UsesIt()
    {
        LetMaxParallelismBe(2);

        var result = _sut.Create(_optionsMock.Object, SomeOutputPath, SomeFiles);

        Assert.Equal(2, result.MaxDegreeOfParallelism);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_MaxParallelismIsNotPositive_UsesProcessorCount(int? maxParallelism)
    {
        LetMaxParallelismBe(maxParallelism);

        var result = _sut.Create(_optionsMock.Object, SomeOutputPath, SomeFiles);

        Assert.Equal(SomeProcessorCount, result.MaxDegreeOfParallelism);
        _loggerMock.Verify(logger =>
            logger.Warning("Maximum parallelism must be a positive number. Using {ProcessorCount} instead", SomeProcessorCount),
            maxParallelism is null ? Times.Never() : Times.Once());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_FollowSymbolicLinksProvided_UsesItAndWarnsWhenSet(bool followSymbolicLinks)
    {
        _optionsMock
            .SetupGet(options => options.FollowSymbolicLinks)
            .Returns(followSymbolicLinks);

        var result = _sut.Create(_optionsMock.Object, SomeOutputPath, SomeFiles);

        Assert.Equal(followSymbolicLinks, result.FollowSymbolicLinks);
        _loggerMock.Verify(logger =>
            logger.Warning("Symbolic links and junctions below the destination path are followed"),
            followSymbolicLinks ? Times.Once() : Times.Never());
    }

    private void LetMaxParallelismBe(int? maxParallelism) =>
        _optionsMock
            .SetupGet(options => options.MaxParallelism)
            .Returns(maxParallelism);
}
