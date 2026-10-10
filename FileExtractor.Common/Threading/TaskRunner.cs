namespace FileExtractor.Common.Threading;

public sealed class TaskRunner : ITaskRunner
{
    public Task ForEachAsync<T>(
        IEnumerable<T> source,
        int maxDegreeOfParallelism,
        Func<T, CancellationToken, ValueTask> body,
        CancellationToken cancellationToken) =>
        Parallel.ForEachAsync(
            source,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            body);
}
