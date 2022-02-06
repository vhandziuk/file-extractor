using FileExtractor.Utils.Compression;
using FileExtractor.Utils.FileSystem;
using StrongInject;

namespace FileExtractor.DiRegistration;

[Register(typeof(FileSystemUtils), Scope.SingleInstance, typeof(IFileSystemUtils))]
[Register(typeof(ZipFileExtractor), Scope.SingleInstance, typeof(IZipFileExtractor))]
[Register(typeof(ZipFileUtils), Scope.SingleInstance, typeof(IZipFileUtils))]
internal sealed class FileExtractorUtilsModule
{
}
