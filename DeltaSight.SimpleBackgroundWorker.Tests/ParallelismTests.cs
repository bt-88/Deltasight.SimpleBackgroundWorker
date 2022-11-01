using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeltaSight.SimpleBackgroundWorker.Tests;

public class Tests
{
    [Test]
    public async Task Test_WithOneDegreeOfParallelism()
    {
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSimpleBackgroundWorker(options => options.MaxDegreesOfParallelism = 1);
            })
            .Build();

        await host.StartAsync();

        var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();

        var now = DateTime.Now;
        
        await bgWorker.QueueAsync(t =>
        {
            AssertTimeSpanIs(now, TimeSpan.FromSeconds(0));

            return Task.CompletedTask;
        });
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t =>
        {
            AssertTimeSpanIs(now, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        });
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t =>
        {
            AssertTimeSpanIs(now, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        });
    }
    
    [Test]
    public async Task Test_WithTwoDegreesOfParallelism()
    {
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSimpleBackgroundWorker(options => options.MaxDegreesOfParallelism = 2);
            })
            .Build();

        await host.StartAsync();

        var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();

        var now = DateTime.Now;
        
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t =>
        {
            AssertTimeSpanIs(now, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        });
        
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t =>
        {
            AssertTimeSpanIs(now, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        });
        await bgWorker.QueueAsync(t => Task.Delay(1000, t));
        await bgWorker.QueueAsync(t =>
        {
            AssertTimeSpanIs(now, TimeSpan.FromSeconds(3));

            return Task.CompletedTask;
        });
    }

    private static void AssertTimeSpanIs(DateTime start, TimeSpan ts)
    {
        Assert.That(ts, Is.EqualTo(DateTime.Now - start).Within(ts * .1));
    }
}