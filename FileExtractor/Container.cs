using FileExtractor.Data;
using FileExtractor.Utils;
using StrongInject;

namespace FileExtractor;

[Register(typeof(FileSystemUtils), Scope.SingleInstance, typeof(IFileSystemUtils))]
[Register(typeof(ZipFileUtils), Scope.SingleInstance, typeof(IZipFileUtils))]
[Register(typeof(ZipFileExtractor), Scope.SingleInstance, typeof(IZipFileExtractor))]
[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
[Register(typeof(TaskRunner), Scope.SingleInstance, typeof(ITaskRunner))]
[Register(typeof(App), Scope.SingleInstance, typeof(IApp))]
internal partial class Container : IAsyncContainer<IApp>
{
}