using Plant01.Upper.Presentation.Core.Services;

namespace Plant01.Upper.Wpf.Services;

public class WpfDispatcherService : IDispatcherService
{
    public void Invoke(Action action)
    {
        // 检查是否已有 Application 实例，防止设计时或测试时崩溃
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        
        if (dispatcher != null && !dispatcher.HasShutdownStarted && !dispatcher.HasShutdownFinished)
        {
            try
            {
                dispatcher.Invoke(action);
            }
            catch (TaskCanceledException)
            {
                // Dispatcher 正在关闭，忽略此操作
            }
        }
        else
        {
            // 如果没有 Dispatcher (如单元测试) 或已关闭，直接执行
            action();
        }
    }

    public Task InvokeAsync(Func<Task> action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        
        if (dispatcher != null && !dispatcher.HasShutdownStarted && !dispatcher.HasShutdownFinished)
        {
            try
            {
                return dispatcher.InvokeAsync(action).Task.Unwrap();
            }
            catch (TaskCanceledException)
            {
                // Dispatcher 正在关闭，返回已完成的任务
                return Task.CompletedTask;
            }
        }
        
        return action();
    }
}
