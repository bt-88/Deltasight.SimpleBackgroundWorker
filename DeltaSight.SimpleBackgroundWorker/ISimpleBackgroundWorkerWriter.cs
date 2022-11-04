namespace DeltaSight.SimpleBackgroundWorker;

public interface ISimpleBackgroundWorkerWriter
{
    ValueTask QueueAsync(Func<CancellationToken, Task> workItem);

    bool TryQueue(Func<CancellationToken, Task> workItem);
}