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

var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();

// Create some work items
var workItems = Enumerable.Range(1, 10)
      .Select(i => new BackgroundWorkItem(
         // Describe the work that must be executed
         async cancellationToken =>
         {
             Console.WriteLine($"[{DateTime.Now}] Job {i}: Doing some heavy lifting in the background, baby!");
             await Task.Delay(1000, cancellationToken);
             Console.WriteLine($"[{DateTime.Now}] Job {i}: Done and dusted");
         },
         // Optional: Provide a name
         $"Job {i}",
         // Optional: Error call back
         e => 
         {
            Console.Writeline($"Oops: {e.Message}");
            
            return Task.Completed;
         })
      .ToArray();

// Dispatch
await bgWorker.QueueAsync(workItems);
// or: bgWorker.TryQueue(workItems);

// Watch the work being done (2 at the same time at most)
Console.Read();
```
