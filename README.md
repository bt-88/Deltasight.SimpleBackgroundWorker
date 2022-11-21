# Deltasight.SimpleBackgroundWorker
A very simple and light weight background worker queue that handles work while respecting a maximum degree of parallelism.

Use this to handle any kind of _fire and forget_ type of work (such as sending notifications, storing items, logging stuff) in your dotnet Console, ASP or Desktop apps.
## Usage
### Adding SimpleBackgroundWorker
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
```
### Adding work
```csharp
var bgWorker = host.Services.GetRequiredService<ISimpleBackgroundWorkerWriter>();

// Create some work tiems
var workItems = Enumerable.Range(1, 10)
      .Select(i => BackgroundWorkItem.Create(
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

// Add the work to our worker
await bgWorker.QueueAsync(workItems);
// or: bgWorker.TryQueue(workItems);

// Watch the work being executed (by pairs of 2)
Console.Read();
```
