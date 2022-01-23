namespace FileExtractor.Utils;

internal interface ITaskRunner
{
    Task<T> Run<T>(Func<T> func);
}