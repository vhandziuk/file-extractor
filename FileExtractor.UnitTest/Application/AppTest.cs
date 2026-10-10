using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor.Application;
using FileExtractor.Application.Commands;
using FileExtractor.Application.Options;
using FileExtractor.Common.Logging;
using Moq;
using Xunit;

namespace FileExtractor.UnitTest.Application;

public class AppTest
{
    private readonly Mock<IConfigurationCommand> _configurationCommandMock = new();
    private readonly Mock<IExtractCommand> _extractCommandMock = new();
    private readonly Mock<IListCommand> _listCommandMock = new();
    private readonly Mock<ILogger<App>> _loggerMock = new();
    private readonly Mock<IConfigurationOptions> _configurationOptionsMock = new();

    private readonly App _sut;

    public AppTest()
    {
        _sut = new App(
            _configurationCommandMock.Object,
            _extractCommandMock.Object,
            _listCommandMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_ConfigurationOptionsProvided_RunsConfigurationCommand()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await _sut.RunAsync(_configurationOptionsMock.Object, cancellationTokenSource.Token);

        _configurationCommandMock.Verify(command =>
            command.RunAsync(_configurationOptionsMock.Object, cancellationTokenSource.Token), Times.Once);
        _extractCommandMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ExtractOptionsProvided_RunsExtractCommand()
    {
        var extractOptions = Mock.Of<IExtractOptions>();
        using var cancellationTokenSource = new CancellationTokenSource();

        await _sut.RunAsync(extractOptions, cancellationTokenSource.Token);

        _extractCommandMock.Verify(command =>
            command.RunAsync(extractOptions, cancellationTokenSource.Token), Times.Once);
        _configurationCommandMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_ListOptionsProvided_RunsListCommand()
    {
        var listOptions = Mock.Of<IListOptions>();
        using var cancellationTokenSource = new CancellationTokenSource();

        await _sut.RunAsync(listOptions, cancellationTokenSource.Token);

        _listCommandMock.Verify(command =>
            command.RunAsync(listOptions, cancellationTokenSource.Token), Times.Once);
        _configurationCommandMock.VerifyNoOtherCalls();
        _extractCommandMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunAsync_OptionsOfUnknownVerbProvided_LogsException()
    {
        await RunAsync(Mock.Of<ICommonOptions>());

        _loggerMock.Verify(logger =>
            logger.Error(It.IsAny<ArgumentOutOfRangeException>(), "An exception occurred. The program will now exit"), Times.Once);
    }

    [Fact]
    public async Task RunAsync_CommandThrows_LogsException()
    {
        var expectedException = new IOException();
        LetConfigurationCommandThrow(expectedException);

        await RunAsync(_configurationOptionsMock.Object);

        _loggerMock.Verify(logger =>
            logger.Error(expectedException, "An exception occurred. The program will now exit"), Times.Once);
    }

    [Fact]
    public async Task RunAsync_CommandCancelled_LogsWarning()
    {
        LetConfigurationCommandThrow(new OperationCanceledException());

        await RunAsync(_configurationOptionsMock.Object);

        _loggerMock.Verify(logger =>
            logger.Warning("Extraction was cancelled. The program will now exit"), Times.Once);
        _loggerMock.Verify(logger =>
            logger.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    private ValueTask RunAsync(ICommonOptions options) =>
        _sut.RunAsync(options, TestContext.Current.CancellationToken);

    private void LetConfigurationCommandThrow(Exception exception) =>
        _configurationCommandMock
            .Setup(command => command.RunAsync(It.IsAny<IConfigurationOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
}
