using StrongInject;

namespace FileExtractor.Data.DiRegistration;

[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
public sealed class FileExtractorDataModule
{
}
