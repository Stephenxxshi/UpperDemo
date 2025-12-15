using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Models.Logging;
using Plant01.Upper.Presentation.Core.Services;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly ILogger<DashboardViewModel> _logger;
        private readonly ILogStore _logStore;
        private readonly IDispatcherService _dispatcherService;
        private bool _disposed;

        [ObservableProperty]
        private bool _isPaused;

        public ObservableCollection<LogItem> Logs { get; } = new();
        public DashboardViewModel() { }
        public DashboardViewModel(ILogger<DashboardViewModel> logger,ILogStore logStore,IDispatcherService dispatcherService)
        {
            _logger = logger;
            _logStore = logStore;
            _dispatcherService = dispatcherService;

            _logStore.LogAdded += _logStore_LogAdded;
            _logger.LogInformation("DashboardViewModel initialized.");
        }

        private void _logStore_LogAdded(LogModel model)
        {
            // 如果 ViewModel 已释放，忽略日志更新
            if (_disposed) return;

            // 确保在 UI 线程更新集合
            _dispatcherService.Invoke(() =>
            {
                // 双重检查，防止在 Invoke 排队期间 ViewModel 被释放
                if (_disposed) return;

                Logs.Add(new LogItem
                {
                    Timestamp = model.Timestamp,
                    // 将 Microsoft.Extensions.Logging.LogLevel 映射到 AntLog 的 LogSeverity
                    Severity = model.Level,
                    Message = model.Message,
                    Exception = model.Exception,
                    Source = model.Category
                });
            });
        }

        [RelayCommand]
        private void Clear()
        {
            _dispatcherService.Invoke(() => Logs.Clear());
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _logStore.LogAdded -= _logStore_LogAdded;
        }
    }
}
