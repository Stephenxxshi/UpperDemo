namespace Plant01.Upper.Presentation.Core.Services;

/// <summary>
/// UI 线程调度器接口，用于解耦 ViewModel 和具体 UI 框架
/// </summary>
public interface IDispatcherService
{
    void Invoke(Action action);
    Task InvokeAsync(Func<Task> action);
}
