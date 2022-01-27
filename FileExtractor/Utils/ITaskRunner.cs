namespace FileExtractor.Utils;

internal interface ITaskRunner
{
    Task Run(Action action);
}