namespace FileExtractor.Common.Threading;

public sealed class TaskRunner : ITaskRunner
{
    public Task Run(Action action) => Task.Run(action);
}