using System.Runtime.CompilerServices;

namespace DeltaSight.SimpleBackgroundWorker;

public struct BackgroundWorkItem
{
    private BackgroundWorkItem(Func<CancellationToken, Task> execute, string name, Func<Exception, Task>? onError, bool ignoreParallelism, TaskCreationOptions options, TimeSpan? cancelAfter)
    {
        Name = name;
        Execute = execute;
        OnError = onError;
        Options = options;
        CancelAfter = cancelAfter;
        IgnoreParallelism = ignoreParallelism;
    }

    public Guid Guid { get; } = Guid.NewGuid();
    
    public string Name { get; }
    public Func<CancellationToken, Task> Execute { get; }
    public Func<Exception, Task>? OnError { get; } = null;
    
    public TaskCreationOptions Options { get; }
    
    public bool IgnoreParallelism { get; }

    /// <summary>
    /// If set, the job will be cancelled automatically if the execution takes longer
    /// </summary>
    public TimeSpan? CancelAfter { get; }

    public static BackgroundWorkItem Create(Func<CancellationToken, Task> executeWork,
        string? name = null,
        Func<Exception, Task>? errorCallback = null,
        bool ignoreParallelism = false,
        TaskCreationOptions options = TaskCreationOptions.None,
        TimeSpan? cancelAfter = null,
        [CallerMemberName] string callerMemberName = "",
        [CallerArgumentExpression("executeWork")] string callerArgumentExpression = "")
    {
        return new BackgroundWorkItem(executeWork, name ?? $"{callerMemberName}::{callerArgumentExpression}", errorCallback, ignoreParallelism, options, cancelAfter);
    }
}