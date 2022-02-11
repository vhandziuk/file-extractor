using FileExtractor.Application;
using FileExtractor.DiRegistration.Modules;
using FileExtractor.Utils.DiRegistration;
using StrongInject;

namespace FileExtractor.DiRegistration;

[RegisterModule(typeof(FileExtractorCommonModule))]
[RegisterModule(typeof(FileExtractorDataModule))]
[RegisterModule(typeof(FileExtractorUtilsModule))]
[Register(typeof(App), Scope.SingleInstance, typeof(IApp))]
internal partial class Container : IAsyncContainer<IApp>
{
}