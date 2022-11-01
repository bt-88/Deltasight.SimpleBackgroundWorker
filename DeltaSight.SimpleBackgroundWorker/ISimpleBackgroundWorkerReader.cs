namespace DeltaSight.SimpleBackgroundWorker;

public interface ISimpleBackgroundWorkerReader
{
    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}