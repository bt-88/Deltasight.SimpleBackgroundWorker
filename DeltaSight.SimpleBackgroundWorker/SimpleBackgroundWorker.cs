using System.Threading.Channels;

namespace DeltaSight.SimpleBackgroundWorker;

public class SimpleBackgroundWorker : ISimpleSimpleSimpleBackgroundWorker
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public SimpleBackgroundWorker(UnboundedChannelOptions? options)
    {
        _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>(options ?? new UnboundedChannelOptions());
    }

    public ValueTask QueueAsync(Func<CancellationToken, Task> workItem)
    {
        return _queue.Writer.WriteAsync(workItem);
    }

    public ValueTask<Func<CancellationToken,Task>> DequeueAsync(CancellationToken cancellationToken)
        => _queue.Reader.ReadAsync(cancellationToken);
}