using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Presentation.Bootstrapper;

using System.Windows;

namespace Plant01.Upper.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IHost? _host;
    private const string MutexName = "Plant01.Upper.Wpf.SingleInstance";
    private Mutex? _mutex;

    public static IHost Host => _host ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        // 使用 Bootstrapper 创建基础 Host，并添加 WPF 特有的服务
        return Bootstrapper.CreateHostBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<MainWindow>();
            });
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("应用程序已经在运行中。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        RegisterEvents();

        await Host.StartAsync();

        var mainWindow = Host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private void RegisterEvents()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        HandleException(e.Exception, "UI Thread Exception");
        e.Handled = true;
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception, "Task Exception");
        e.SetObserved();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        HandleException(e.ExceptionObject as Exception, "AppDomain Exception");
    }

    private void HandleException(Exception? exception, string source)
    {
        if (exception == null) return;

        try
        {
            var logger = Host.Services.GetService<ILogger<App>>();
            logger?.LogError(exception, "Unhandled exception from {Source}", source);
        }
        catch
        {
            // 如果 Logger 获取失败，忽略
        }

        string message = $"发生未处理异常 ({source}):\n{exception.Message}\n\n请联系管理员。";
        MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }
}
