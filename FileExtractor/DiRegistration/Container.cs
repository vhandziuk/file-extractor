using FileExtractor.Application;
using FileExtractor.Application.Commands;
using FileExtractor.Common.DiRegistration;
using FileExtractor.Data.DiRegistration;
using FileExtractor.Utils.DiRegistration;
using StrongInject;

namespace FileExtractor.DiRegistration;

[RegisterModule(typeof(FileExtractorCommonModule))]
[RegisterModule(typeof(FileExtractorDataModule))]
[RegisterModule(typeof(FileExtractorUtilsModule))]
[Register(typeof(App), Scope.SingleInstance, typeof(IApp))]
[Register(typeof(ConfigurationCommand), Scope.SingleInstance, typeof(IConfigurationCommand))]
[Register(typeof(ExtractionSettingsFactory), Scope.SingleInstance, typeof(IExtractionSettingsFactory))]
internal partial class Container : IAsyncContainer<IApp>
{
}