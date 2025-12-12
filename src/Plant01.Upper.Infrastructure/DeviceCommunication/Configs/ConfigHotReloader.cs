using Microsoft.Extensions.Logging;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Configs;

public class ConfigHotReloader : IDisposable
{
    private readonly string _configsPath;
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<ConfigHotReloader> _logger;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

    public event EventHandler? ConfigChanged;

    public ConfigHotReloader(string configsPath, ILogger<ConfigHotReloader> logger)
    {
        _configsPath = configsPath;
        _logger = logger;

        _watcher = new FileSystemWatcher(_configsPath)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Only care about tags.csv or json files in Channels folder
        if (e.Name != null && 
            (e.Name.EndsWith("tags.csv", StringComparison.OrdinalIgnoreCase) || 
             (e.Name.Contains("Channels") && e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))))
        {
            DebounceReload();
        }
    }

    private void DebounceReload()
    {
        lock (_lock)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(500, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    _logger.LogInformation("Configuration change detected. Triggering reload...");
                    ConfigChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
    }
}
