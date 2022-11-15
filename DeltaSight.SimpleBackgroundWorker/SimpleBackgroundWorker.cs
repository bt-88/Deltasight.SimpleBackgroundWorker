using System.Threading.Channels;

namespace DeltaSight.SimpleBackgroundWorker;

public class SimpleBackgroundWorker : ISimpleBackgroundWorker
{
    private readonly Channel<BackgroundWorkItem> _queue;

    public SimpleBackgroundWorker(UnboundedChannelOptions? options)
    {
        _queue = Channel.CreateUnbounded<BackgroundWorkItem>(options ?? new UnboundedChannelOptions());
    }
    
    public ValueTask QueueAsync(BackgroundWorkItem workItem)
        => _queue.Writer.WriteAsync(workItem);
    
    public bool TryQueue(BackgroundWorkItem workItem)
        => _queue.Writer.TryWrite(workItem);

    public int TryQueue(IEnumerable<BackgroundWorkItem> workItems) => workItems.Count(TryQueue);

    public async ValueTask QueueAsync(IEnumerable<BackgroundWorkItem> workItems)
    {
        foreach (var item in workItems)
        {
            await QueueAsync(item);
        }
    }


    public ValueTask<BackgroundWorkItem> DequeueAsync(CancellationToken cancellationToken)
        => _queue.Reader.ReadAsync(cancellationToken);
}