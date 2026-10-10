namespace FileExtractor.Common.Threading;

public interface ITaskRunner
{
    /// <summary>
    /// Invokes <paramref name="body"/> for every item of <paramref name="source"/> on the thread pool,
    /// running at most <paramref name="maxDegreeOfParallelism"/> invocations at the same time.
    /// </summary>
    Task ForEachAsync<T>(
        IEnumerable<T> source,
        int maxDegreeOfParallelism,
        Func<T, CancellationToken, ValueTask> body,
        CancellationToken cancellationToken);
}
