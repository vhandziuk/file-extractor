using FileExtractor.Common.Logging;

using FileExtractor.Data;
using FileExtractor.Utils;
using StrongInject;

namespace FileExtractor;

[Register(typeof(App), Scope.SingleInstance, typeof(IApp))]
[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
[Register(typeof(FileSystemUtils), Scope.SingleInstance, typeof(IFileSystemUtils))]
[Register(typeof(SerilogLogger<>), Scope.SingleInstance, typeof(ILogger<>))]
[Register(typeof(TaskRunner), Scope.SingleInstance, typeof(ITaskRunner))]
[Register(typeof(ZipFileExtractor), Scope.SingleInstance, typeof(IZipFileExtractor))]
[Register(typeof(ZipFileUtils), Scope.SingleInstance, typeof(IZipFileUtils))]
internal partial class Container : IAsyncContainer<IApp>
{
}