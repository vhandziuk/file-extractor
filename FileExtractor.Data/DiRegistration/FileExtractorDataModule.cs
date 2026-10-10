using StrongInject;

namespace FileExtractor.Data.DiRegistration;

[Register(typeof(TextFileWrapper), Scope.SingleInstance, typeof(ITextFile))]
[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
public sealed class FileExtractorDataModule
{
}
