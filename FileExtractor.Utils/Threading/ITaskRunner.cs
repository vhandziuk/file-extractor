namespace FileExtractor.Utils.Threading;

public interface ITaskRunner
{
    Task Run(Action action);
}