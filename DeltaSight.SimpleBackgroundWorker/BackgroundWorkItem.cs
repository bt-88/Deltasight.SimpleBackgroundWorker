namespace DeltaSight.SimpleBackgroundWorker;

public struct BackgroundWorkItem
{
    public BackgroundWorkItem(Func<CancellationToken,Task> execute, string name = "Untitled", Func<Exception, Task>? onError = null)
    {
        Name = name;
        Execute = execute;
        OnError = onError;
    }

    public string Name { get; }
    public Func<CancellationToken, Task> Execute { get; }
    public Func<Exception, Task>? OnError { get; } = null;
}