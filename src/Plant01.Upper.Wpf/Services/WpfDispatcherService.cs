using Plant01.Upper.Presentation.Core.Services;

namespace Plant01.Upper.Wpf.Services;

public class WpfDispatcherService : IDispatcherService
{
    public void Invoke(Action action)
    {
        // 检查是否已有 Application 实例，防止设计时或测试时崩溃
        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }
        else
        {
            action(); // 如果没有 Dispatcher (如单元测试)，直接执行
        }
    }

    public Task InvokeAsync(Func<Task> action)
    {
        if (System.Windows.Application.Current?.Dispatcher != null)
        {
            return System.Windows.Application.Current.Dispatcher.InvokeAsync(action).Task.Unwrap();
        }
        return action();
    }
}
