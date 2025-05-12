namespace Mimeo.DynamicUI.Blazor.Services;

public class TaskRunningService
{
    public event Action<bool>? OnTaskRunningChanged;

    private int runningTasksCount;

    /// <summary>
    /// Runs the given task and notifies UI components that a task is running and when it's complete
    /// </summary>
    public async Task<T> Run<T>(Func<Task<T>> task)
    {
        StartRunning();
        try
        {
            return await task();
        }
        finally
        {
            StopRunning();
        }
    }

    /// <summary>
    /// Runs the given task and notifies UI components that a task is running and when it's complete
    /// </summary>
    public async Task Run(Func<Task> task)
    {
        StartRunning();
        try
        {
            await task();
        }
        finally
        {
            StopRunning();
        }
    }

    private void StartRunning()
    {
        Interlocked.Increment(ref runningTasksCount);

        CheckIsRunning();
    }

    private void StopRunning()
    {
        Interlocked.Decrement(ref runningTasksCount);

        CheckIsRunning();
    }

    private void CheckIsRunning()
    {
        var taskRunning = runningTasksCount > 0;
        OnTaskRunningChanged?.Invoke(taskRunning);
    }
}