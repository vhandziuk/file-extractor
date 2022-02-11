using FileExtractor.Utils.Compression;
using FileExtractor.Utils.FileSystem;
using StrongInject;

namespace FileExtractor.Utils.DiRegistration;

[Register(typeof(FileSystemUtils), Scope.SingleInstance, typeof(IFileSystemUtils))]
[Register(typeof(ZipFileExtractor), Scope.SingleInstance, typeof(IZipFileExtractor))]
[Register(typeof(ZipFileUtils), Scope.SingleInstance, typeof(IZipFileUtils))]
[Register(typeof(ZipFileWrapper), Scope.SingleInstance, typeof(IZipFile))]
public sealed class FileExtractorUtilsModule
{
}
