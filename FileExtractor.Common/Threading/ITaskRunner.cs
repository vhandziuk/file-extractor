namespace FileExtractor.Common.Threading;

public interface ITaskRunner
{
    Task<T> Run<T>(Func<T> func);
    Task<T> Run<T>(Func<Task<T>> func);
}