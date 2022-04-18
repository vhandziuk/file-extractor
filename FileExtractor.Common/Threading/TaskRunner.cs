namespace FileExtractor.Common.Threading;

public sealed class TaskRunner : ITaskRunner
{
    public Task<T> Run<T>(Func<T> func) => Task.Run(func);

    public Task<T> Run<T>(Func<Task<T>> func) => Task.Run(func);
}