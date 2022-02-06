namespace FileExtractor.Common.Threading;

public interface ITaskRunner
{
    Task Run(Action action);
}