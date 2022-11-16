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

    private async Task Run(Guid guid, BackgroundWorkItem workItem, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting work item {Name} ({Guid})", workItem.Name, guid);

            var sw = Stopwatch.StartNew();

            await workItem.Execute(stoppingToken).ConfigureAwait(false);

            sw.Stop();

            _logger.LogInformation("Completed work item {Name} ({Guid}) in {Ms}ms", workItem.Name, guid,
                sw.ElapsedMilliseconds);

            // Remove myself from the 'running' jobs
            _running.Remove(guid, out _);
        }
        catch (OperationCanceledException)
        {
            // Ignore exception that is thrown on cancelling
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Work item {Name} ({Guid}) failed with message: {Message}", workItem.Name, guid,
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
                _logger.LogError(e2, "The error callback for {Name} ({Guid}) faulted", workItem.Name, guid);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);
         
            var guid = Guid.NewGuid();

            _logger.LogInformation(
                "Received new work item {Name} ({Guid}) with {AvailableWorkerCount} available workers", workItem.Name, guid,
                _semaphore.CurrentCount);
            
            // Wait for free capacity
            await _semaphore.WaitAsync(stoppingToken);

            // Create a job and add it to the dictionary of 'running' jobs
            _running.TryAdd(guid, Run(guid, workItem, stoppingToken));
        }
        
        // Wait for the running jobs to finish
        _logger.LogInformation("Stopped receiving new work items and waiting for running jobs to finish...");
        
        await Task.WhenAll(_running.Values);
    }
}
