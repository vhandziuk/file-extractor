using FileExtractor.Common.Logging;
using FileExtractor.Common.Threading;
using StrongInject;

namespace FileExtractor.DiRegistration.Modules;

[Register(typeof(SerilogLogger<>), Scope.SingleInstance, typeof(ILogger<>))]
[Register(typeof(TaskRunner), Scope.SingleInstance, typeof(ITaskRunner))]
internal sealed class FileExtractorCommonModule
{
}
