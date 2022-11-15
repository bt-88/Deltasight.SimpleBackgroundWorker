namespace DeltaSight.SimpleBackgroundWorker;

public interface ISimpleBackgroundWorkerReader
{
    ValueTask<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken);
}