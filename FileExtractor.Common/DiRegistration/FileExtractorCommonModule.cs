using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using StrongInject;

namespace FileExtractor.Common.DiRegistration;

[Register(typeof(SerilogLogger<>), Scope.SingleInstance, typeof(ILogger<>))]
[Register(typeof(TaskRunner), Scope.SingleInstance, typeof(ITaskRunner))]
public sealed class FileExtractorCommonModule
{
}
