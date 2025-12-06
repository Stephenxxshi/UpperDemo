using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Presentation.Bootstrapper;
using Plant01.Upper.Presentation.Core.Services;
using Plant01.Upper.Wpf.Services;
using Plant01.Upper.Wpf.Views;
using Plant01.Upper.Application.Interfaces;

using System.Windows;

namespace Plant01.Upper.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
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
                services.AddSingleton<IDialogService, Plant01.Upper.Wpf.Services.DialogService>();

                // 注册 Dispatcher 服务
                services.AddSingleton<IDispatcherService,WpfDispatcherService>();

                // 注册 Views
                services.AddTransient<MesDebugView>();

                services.AddLogging(configure =>
                {
                    configure.AddDebug();
                });
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

        // 🔥 自动启动 MES Web API 服务
        await StartMesWebApiAsync();

        var mainWindow = Host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// 启动 MES Web API 服务
    /// </summary>
    private async Task StartMesWebApiAsync()
    {
        try
        {
            // 确保 MesCommandService 被实例化并订阅事件
            var mesCommandService = Host.Services.GetRequiredService<IMesCommandService>();
            
            var mesWebApi = Host.Services.GetRequiredService<IMesWebApi>();
            var logger = Host.Services.GetService<ILogger<App>>();
            
            if (!mesWebApi.IsRunning)
            {
                await mesWebApi.StartAsync();
                logger?.LogInformation("MES Web API 服务已自动启动");
            }
        }
        catch (Exception ex)
        {
            var logger = Host.Services.GetService<ILogger<App>>();
            logger?.LogError(ex, "自动启动 MES Web API 服务失败");
            
            // 不阻止应用启动，仅记录错误
        }
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
            // 🔥 停止 MES Web API 服务
            await StopMesWebApiAsync();
            
            await _host.StopAsync();
            _host.Dispose();
        }

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }

    /// <summary>
    /// 停止 MES Web API 服务
    /// </summary>
    private async Task StopMesWebApiAsync()
    {
        try
        {
            var mesWebApi = _host?.Services.GetRequiredService<IMesWebApi>();
            var logger = _host?.Services.GetService<ILogger<App>>();
            
            if (mesWebApi != null && mesWebApi.IsRunning)
            {
                await mesWebApi.StopAsync();
                logger?.LogInformation("MES Web API 服务已自动停止");
            }
        }
        catch (Exception ex)
        {
            var logger = _host?.Services.GetService<ILogger<App>>();
            logger?.LogError(ex, "自动停止 MES Web API 服务失败");
        }
    }
}
