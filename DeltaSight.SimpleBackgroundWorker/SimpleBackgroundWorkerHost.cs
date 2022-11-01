using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeltaSight.SimpleBackgroundWorker;

public class SimpleBackgroundWorkerHost : BackgroundService
{
    private readonly ISimpleBackgroundWorkerReader _queue;
    private readonly ILogger<SimpleBackgroundWorkerHost> _logger;
    private readonly SemaphoreSlim _semaphore;

    public SimpleBackgroundWorkerHost(ISimpleBackgroundWorkerReader queue, IOptions<SimpleBackgroundWorkerHostOptions> options, ILogger<SimpleBackgroundWorkerHost> logger)
    {
        var mdop = options.Value.MaxDegreesOfParallelism;
        
        _queue = queue;
        _logger = logger;
        _semaphore = new SemaphoreSlim(mdop, mdop);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var id = Guid.NewGuid().ToString("N")[..6];
            
            try
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                
                _logger.LogInformation("Received new work item {Id} with {AvailableWorkerCount} available workers", id, _semaphore.CurrentCount);

                try
                {
                    await _semaphore.WaitAsync(stoppingToken);

                    _logger.LogInformation("Start execution of work item {Id}", id);

                    var sw = Stopwatch.StartNew();

                    await workItem(stoppingToken);

                    sw.Stop();

                    _logger.LogInformation("Completed work item {Id} in {Ms}ms", id, sw.ElapsedMilliseconds);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore exception that is thrown on cancelling
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Work item {Id} failed with message: {Message}", id, e.Message);
            }
        }
    }
}
