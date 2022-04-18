using FileExtractor.Utils.Compression;
using FileExtractor.Utils.Compression.Other;
using FileExtractor.Utils.Compression.Rar;
using FileExtractor.Utils.Compression.SevenZip;
using FileExtractor.Utils.Compression.Zip;
using FileExtractor.Utils.FileSystem;
using StrongInject;

namespace FileExtractor.Utils.DiRegistration;

[Register(typeof(FileSystemUtils), Scope.SingleInstance, typeof(IFileSystemUtils))]
[Register(typeof(ZipFileExtractor), Scope.SingleInstance, typeof(IZipFileExtractor))]
[Register(typeof(ZipFileUtils), Scope.SingleInstance, typeof(IZipFileUtils))]
[Register(typeof(ZipFileWrapper), Scope.SingleInstance, typeof(IZipFile))]
[Register(typeof(RarFileExtractor), Scope.SingleInstance, typeof(IRarFileExtractor))]
[Register(typeof(RarFileUtils), Scope.SingleInstance, typeof(IRarFileUtils))]
[Register(typeof(RarFileWrapper), Scope.SingleInstance, typeof(IRarFile))]
[Register(typeof(SevenZipFileExtractor), Scope.SingleInstance, typeof(ISevenZipFileExtractor))]
[Register(typeof(SevenZipFileUtils), Scope.SingleInstance, typeof(ISevenZipFileUtils))]
[Register(typeof(SevenZipFileWrapper), Scope.SingleInstance, typeof(ISevenZipFile))]
[Register(typeof(OtherArchiveFileExtractor), Scope.SingleInstance, typeof(IOtherArchiveFileExtractor))]
[Register(typeof(ArchiveExtractorFactory), Scope.SingleInstance, typeof(IArchiveExtractorFactory))]
[Register(typeof(ArchiveExtractor), Scope.SingleInstance, typeof(IArchiveExtractor))]
public sealed class FileExtractorUtilsModule
{
}
