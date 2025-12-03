using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Models.Logging;
using Plant01.Upper.Presentation.Core.Services;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels
{
    public class DashboardViewModel
    {
        private readonly ILogger<DashboardViewModel> _logger;
        private readonly ILogStore _logStore;
        private readonly IDispatcherService _dispatcherService;

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
            // 确保在 UI 线程更新集合
            _dispatcherService.Invoke(() =>
            {
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
    }
}
