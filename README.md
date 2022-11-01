# Deltasight.SimpleBackgroundWorker
A very simple and light weight background worker queue for your dotnet projects that respects a maximum degree of parallelism.
## Usage
```csharp
// Apply .AddSimpleBackgroundWorker to your service collection
var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                // Specify the maximum degrees of paralellism
                services.AddSimpleBackgroundWorker(options => options.MaxDegreesOfParallelism = 2);
            })
            .Build();

await host.StartAsync();

// Start producing some work
var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();

await bgWorker.QueueAsync(async t =>
  {
      Console.WriteLine($"[{DateTime.Now}] 1: Doing some heavy lifting in the background, baby!");
      await Task.Delay(1000);
      Console.WriteLine($"[{DateTime.Now}] 1: Done and dusted");
  });

await bgWorker.QueueAsync(async t =>
  {
      Console.WriteLine($"[{DateTime.Now}] 2: Doing some heavy lifting in the background, baby!");
      await Task.Delay(1000);
      Console.WriteLine($"[{DateTime.Now}] 2: Done and dusted");
  });

await bgWorker.QueueAsync(async t =>
  {
      Console.WriteLine($"[{DateTime.Now}] 3: Doing some heavy lifting in the background, baby!");
      await Task.Delay(1000);
      Console.WriteLine($"[{DateTime.Now}] 3: Done and dusted");
  });
```
