using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Models.Logging;
using UiLogItem = Plant01.WpfUI.Models.LogItem;
using UiLogSeverity = Plant01.WpfUI.Models.LogSeverity;

namespace wpfuidemo.ViewModels;

public partial class LogViewModel : ObservableObject
{
    private readonly ILogger<LogViewModel> _logger;
    private readonly ILogStore _logStore;
    private readonly DispatcherTimer _timer;

    public ObservableCollection<UiLogItem> Logs { get; } = new();

    [ObservableProperty]
    private bool _isPaused;

    partial void OnIsPausedChanged(bool value)
    {
        _logStore.IsPaused = value;
    }

    public LogViewModel(ILogger<LogViewModel> logger, ILogStore logStore)
    {
        _logger = logger;
        _logStore = logStore;

        // Subscribe to log events
        _logStore.LogAdded += OnLogAdded;
        _logStore.ClearRequested += OnClearRequested;

        // Start simulation
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => GenerateRandomLog();
        _timer.Start();
    }

    private void OnClearRequested()
    {
        Application.Current.Dispatcher.Invoke(() => Logs.Clear());
    }

    [RelayCommand]
    private void Clear()
    {
        _logStore.Clear();
    }

    private void OnLogAdded(LogModel model)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Logs.Add(new UiLogItem
            {
                Timestamp = model.Timestamp,
                Severity = MapSeverity(model.Level),
                Message = model.Message,
                Exception = model.Exception,
                Source = model.Category
            });
            
            // Limit size
            if (Logs.Count > _logStore.MaxItemCount) Logs.RemoveAt(0);
        });
    }

    private UiLogSeverity MapSeverity(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace or LogLevel.Debug => UiLogSeverity.Debug,
            LogLevel.Information => UiLogSeverity.Info,
            LogLevel.Warning => UiLogSeverity.Warning,
            LogLevel.Error or LogLevel.Critical => UiLogSeverity.Error,
            _ => UiLogSeverity.Info
        };
    }

    private void GenerateRandomLog()
    {
        var rng = new Random();
        var type = rng.Next(0, 10);
        if (type < 4) _logger.LogInformation("This is an information log at {Time}", DateTime.Now);
        else if (type < 6) _logger.LogDebug("Debug: Detailed system info here.");
        else if (type < 8) _logger.LogWarning("Warning: Something might be wrong.");
        else if (type < 9) _logger.LogError(new Exception("Simulated Error"), "An error occurred processing request.");
        else _logger.LogInformation("Operation completed successfully.");
    }
}