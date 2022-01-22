namespace FileExtractor
{
    internal sealed class TaskRunner : ITaskRunner
    {
        public Task<T> Run<T>(Func<T> func) => Task.Run(func);
    }
}