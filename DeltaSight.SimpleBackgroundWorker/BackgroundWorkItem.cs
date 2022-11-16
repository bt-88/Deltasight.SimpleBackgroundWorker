namespace DeltaSight.SimpleBackgroundWorker;

public struct BackgroundWorkItem
{
    public BackgroundWorkItem(Func<CancellationToken, Task> execute, string name = "Untitled", Func<Exception, Task>? onError = null, bool isLongRunning = false, TimeSpan? cancelAfter = null)
    {
        Name = name;
        Execute = execute;
        OnError = onError;
        IsLongRunning = isLongRunning;
        CancelAfter = cancelAfter;
    }

    public string Name { get; }
    public Func<CancellationToken, Task> Execute { get; }
    public Func<Exception, Task>? OnError { get; } = null;
    
    /// <summary>
    /// Flag to indicate the task is long running
    /// <remarks>Long running tasks will not impact maximum degree of parallelism
    /// </summary>
    public bool IsLongRunning { get; }

    /// <summary>
    /// If set, the job will be cancelled automatically if the execution takes longer
    /// </summary>
    public TimeSpan? CancelAfter { get; }
}