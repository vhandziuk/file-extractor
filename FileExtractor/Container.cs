using FileExtractor.Data;
using FileExtractor.Utils;
using StrongInject;

namespace FileExtractor;

[Register(typeof(FileSystemUtils), Scope.SingleInstance, typeof(IFileSystemUtils))]
[Register(typeof(ZipFileUtils), Scope.SingleInstance, typeof(IZipFileUtils))]
[Register(typeof(ZipFileExtractor), Scope.SingleInstance, typeof(IZipFileExtractor))]
[Register(typeof(CsvFileNameProvider), Scope.SingleInstance, typeof(ICsvFileNameProvider))]
[Register(typeof(TaskRunner), Scope.SingleInstance, typeof(ITaskRunner))]
[Register(typeof(ExtractFlow), Scope.SingleInstance, typeof(IExtractFlow))]
internal partial class Container : IAsyncContainer<IExtractFlow>
{
}