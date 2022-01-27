namespace FileExtractor.Utils;

internal sealed class TaskRunner : ITaskRunner
{
    public Task Run(Action action) => Task.Run(action);
}