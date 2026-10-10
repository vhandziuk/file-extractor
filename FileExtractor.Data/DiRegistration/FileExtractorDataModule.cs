using StrongInject;

namespace FileExtractor.Data.DiRegistration;

[Register(typeof(TextFileWrapper), Scope.SingleInstance, typeof(ITextFile))]
[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
[Register(typeof(FilePatternParser), Scope.SingleInstance, typeof(IFilePatternParser))]
public sealed class FileExtractorDataModule
{
}
