using System.Runtime.CompilerServices;

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

    public Guid Guid { get; } = Guid.NewGuid();
    
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

    public static BackgroundWorkItem Create(Func<CancellationToken, Task> executeWork,
       [CallerArgumentExpression("executeWork")] string name = "Untitled",
        Func<Exception, Task>? errorCallback = null,
        bool isLongRunning = false,
        TimeSpan? cancelAfter = null
        )
    {
        return new BackgroundWorkItem(executeWork, name, errorCallback, isLongRunning, cancelAfter);
    }
}