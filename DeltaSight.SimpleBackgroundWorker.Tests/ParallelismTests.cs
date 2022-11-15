using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeltaSight.SimpleBackgroundWorker.Tests;

public class Tests
{

    [Test]
    public async Task Test_WithManyDegreesOfParallelism()
    {
        var count = 100;
        var countWorkers = 10;
        var m = count / countWorkers;
        
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSimpleBackgroundWorker(options => options.MaxDegreesOfParallelism = countWorkers);
            })
            .Build();

        await host.StartAsync();

        var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();
        
        var ms = 100;

        var semaphore = new SemaphoreSlim(0, count);

        var ended = new ConcurrentBag<DateTime>();
        
        var jobs = Enumerable.Range(1, count)
            .Select(i => new BackgroundWorkItem(async t =>
            {
                try
                {
                    await Task.Delay(ms, t);
                    ended.Add(DateTime.Now);
                    semaphore.Release();
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }, $"Job {i}", e =>
            {
                Assert.Fail(e.Message);

                return Task.CompletedTask;
            }))
            .ToArray();

        await bgWorker.QueueAsync(jobs);

        var startAt = DateTime.Now;
        
        while (semaphore.CurrentCount < count)
        {
            await Task.Delay(1);
        }

        Assert.Multiple(() =>
        {
            Assert.That(ended, Has.Count.EqualTo(count));
            Assert.That((ended.Max() - startAt).TotalMilliseconds, Is.EqualTo(m * ms).Within(ms));
        });
        
        await host.StopAsync();
        await host.WaitForShutdownAsync();
    }

    private static void AssertTimeSpanIs(DateTime start, TimeSpan ts)
    {
        Assert.That(DateTime.Now - start, Is.EqualTo(ts).Within(ts * .05));
    }
}