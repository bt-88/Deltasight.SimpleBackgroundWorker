using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<Guid, Task> _running = new();

    public SimpleBackgroundWorkerHost(ISimpleBackgroundWorkerReader queue, IOptions<SimpleBackgroundWorkerHostOptions> options, ILogger<SimpleBackgroundWorkerHost> logger)
    {
        var mdop = options.Value.MaxDegreesOfParallelism;
        
        _queue = queue;
        _logger = logger;
        _semaphore = new SemaphoreSlim(mdop, mdop);
    }

    private async Task Run(BackgroundWorkItem workItem, CancellationToken stoppingToken)
    {
        // Wait for worker first
        try
        {
            await WaitForWorker(workItem, stoppingToken);
        }
        catch
        {
            // Remove myself from the 'running' jobs
            _running.Remove(workItem.Guid, out _);
            
            // Stop
            return;
        }
        
        // Now execute work item
        try
        {
            var sw = Stopwatch.StartNew();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            if (workItem.CancelAfter.HasValue)
            {
                cts.CancelAfter(workItem.CancelAfter.Value);
            }

            var task = await Task.Factory.StartNew(
                    () => workItem.Execute(cts.Token),
                    workItem.Options).ConfigureAwait(false);

            await task;
            
            sw.Stop();

            _logger.LogInformation("Completed work item {Name} ({Guid}) in {Ms}ms", workItem.Name, workItem.Guid,
                sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException e)
        {
            if (workItem.CancelAfter is not null)
            {
                _logger.LogError(e, "Work item {Name} ({Guid}) execution cancelled because operation took longer than {CancelAfter}", workItem.Name, workItem.Guid, workItem.CancelAfter.Value);
            }

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Work item {Name} ({Guid}) execution faulted with message: {Message}", workItem.Name, workItem.Guid,
                e.Message);

            try
            {
                if (workItem.OnError is not null)
                {
                    await workItem.OnError(e);
                }
            }
            catch (Exception e2)
            {
                _logger.LogError(e2, "The error callback for {Name} ({Guid}) faulted", workItem.Name, workItem.Guid);
            }
        }
        finally
        {
            // Remove myself from the 'running' jobs
            _running.Remove(workItem.Guid, out _);
            
            // Release worker
            ReleaseWorker(workItem);
        }
    }

    private async ValueTask WaitForWorker(BackgroundWorkItem item, CancellationToken cancellationToken)
    {
        if (item.IgnoreParallelism) return;
        
        await _semaphore.WaitAsync(cancellationToken);
    }

    private void ReleaseWorker(BackgroundWorkItem item)
    {
        if (item.IgnoreParallelism) return;

        _semaphore.Release();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            _logger.LogInformation(
                "Received new work item {Name} ({Guid}) with {AvailableWorkerCount} available workers", workItem.Name, workItem.Guid,
                _semaphore.CurrentCount);

            // Create a job and add it to the dictionary of 'running' jobs
            _running.TryAdd(workItem.Guid, Run(workItem, stoppingToken));
        }
        
        // Wait for the running jobs to finish
        _logger.LogInformation("Stopped receiving new work items and waiting for running jobs to finish...");
        
        await Task.WhenAll(_running.Values);
    }
}
