namespace DeltaSight.SimpleBackgroundWorker;

public interface ISimpleBackgroundWorkerWriter
{
    ValueTask QueueAsync(BackgroundWorkItem workItem);

    bool TryQueue(BackgroundWorkItem workItem);

    int TryQueue(IEnumerable<BackgroundWorkItem> workItems);
    ValueTask QueueAsync(IEnumerable<BackgroundWorkItem> workItems);
}