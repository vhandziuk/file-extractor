using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileExtractor.Common.Threading;
using Xunit;

namespace FileExtractor.Common.UnitTest.Threading;

public class TaskRunnerTest
{
    private readonly TaskRunner _sut = new();

    [Fact]
    public async Task ForEachAsync_ItemsProvided_InvokesBodyForEveryItem()
    {
        var items = Enumerable.Range(0, 50).ToArray();
        var processedItems = new ConcurrentBag<int>();

        await _sut.ForEachAsync(
            items,
            4,
            (item, _) =>
            {
                processedItems.Add(item);
                return ValueTask.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(items, processedItems.Order());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task ForEachAsync_MaxDegreeOfParallelismProvided_NeverExceedsIt(int maxDegreeOfParallelism)
    {
        var running = 0;
        var maxRunning = 0;

        await _sut.ForEachAsync(
            Enumerable.Range(0, 20),
            maxDegreeOfParallelism,
            async (_, cancellationToken) =>
            {
                var current = Interlocked.Increment(ref running);
                InterlockedMax(ref maxRunning, current);
                await Task.Delay(10, cancellationToken);
                Interlocked.Decrement(ref running);
            },
            TestContext.Current.CancellationToken);

        Assert.InRange(maxRunning, 1, maxDegreeOfParallelism);
    }

    [Fact]
    public async Task ForEachAsync_TokenIsCancelled_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var invocationCount = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ForEachAsync(
                Enumerable.Range(0, 10),
                2,
                (_, _) =>
                {
                    Interlocked.Increment(ref invocationCount);
                    return ValueTask.CompletedTask;
                },
                cancellationTokenSource.Token));

        Assert.Equal(0, invocationCount);
    }

    [Fact]
    public async Task ForEachAsync_BodyThrows_PropagatesException()
    {
        var expectedException = new InvalidOperationException();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ForEachAsync(
                Enumerable.Range(0, 10),
                2,
                (item, _) => item == 5 ? throw expectedException : ValueTask.CompletedTask,
                TestContext.Current.CancellationToken));

        Assert.Same(expectedException, exception);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        var current = Volatile.Read(ref target);
        while (value > current)
        {
            var previous = Interlocked.CompareExchange(ref target, value, current);
            if (previous == current)
                return;

            current = previous;
        }
    }
}
