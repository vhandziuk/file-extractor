using FileExtractor.Data;
using StrongInject;

namespace FileExtractor.DiRegistration.Modules;

[Register(typeof(CsvFileInfoProvider), Scope.SingleInstance, typeof(ICsvFileInfoProvider))]
internal sealed class FileExtractorDataModule
{
}
