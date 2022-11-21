using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DeltaSight.SimpleBackgroundWorker.Tests;

public class Tests
{

    [Test]
    public async Task Test_WithManyDegreesOfParallelism()
    {
        const int count = 100;
        const int countWorkers = 10;
        const int m = count / countWorkers;
        const int jobTimeMs = 100;
        
        var semaphore = new SemaphoreSlim(0, count);
        var ended = new ConcurrentBag<DateTime>();
        
        var host = new HostBuilder()
            .ConfigureLogging((ctx, builder) => builder.AddDebug())
            .ConfigureServices(services =>
            {
                services.AddSimpleBackgroundWorker(options => options.MaxDegreesOfParallelism = countWorkers);
            })
            .Build();

        await host.StartAsync();

        var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();
        
        var jobs = Enumerable.Range(1, count)
            .Select(i => BackgroundWorkItem.Create(
                async t =>
                {
                    await Task.Delay(jobTimeMs, t);
                    ended.Add(DateTime.Now);
                    semaphore.Release();
                }, $"Job {i}", e =>
                {
                    Assert.Fail(e.Message);

                    return Task.CompletedTask;
                }))
            .ToArray();

        await bgWorker.QueueAsync(jobs);

        // Queue some long running jobs (that shouldn't interfere with max. degree of parallelism)
        await bgWorker.QueueAsync(Enumerable.Range(1, 10).Select(x => new BackgroundWorkItem(t => Task.Delay(-1, t), isLongRunning:true)));

        var startAt = DateTime.Now;
        
        // Wait for all (short running) jobs to finish
        while (semaphore.CurrentCount < count)
        {
            await Task.Delay(1);
        }

        Assert.Multiple(() =>
        {
            Assert.That(ended, Has.Count.EqualTo(count));
            Assert.That((ended.Max() - startAt).TotalMilliseconds, Is.EqualTo(m * jobTimeMs).Within(jobTimeMs));
        });
        
        await host.StopAsync();
        await host.WaitForShutdownAsync();
    }
}