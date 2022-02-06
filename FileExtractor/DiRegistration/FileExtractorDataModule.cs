using FileExtractor.Data;
using StrongInject;

namespace FileExtractor.DiRegistration;

[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
internal sealed class FileExtractorDataModule
{
}
