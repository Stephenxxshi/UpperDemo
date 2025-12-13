using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

public class Channel : IDisposable
{
    private readonly DeviceConfig _config;
    private readonly IDriver _driver;
    private readonly List<Tag> _tags = new();
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private Task? _pollingTask;

    public string Name => _config.Name;

    private readonly Action<Tag>? _onTagChanged;

    public Channel(DeviceConfig config, IDriver driver, IEnumerable<Tag> tags, ILogger logger, Action<Tag>? onTagChanged = null)
    {
        _config = config;
        _driver = driver;
        _tags.AddRange(tags);
        _logger = logger;
        _onTagChanged = onTagChanged;
    }

    public void Start()
    {
        if (!_config.Enabled) return;

        _cts = new CancellationTokenSource();
        _pollingTask = Task.Run(() => PollingLoop(_cts.Token));
        _logger.LogInformation("Channel {Channel} started", Name);
    }

    public async Task StopAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            if (_pollingTask != null)
            {
                try
                {
                    await _pollingTask;
                }
                catch (OperationCanceledException) { }
            }
            _cts.Dispose();
            _cts = null;
        }
        
        await _driver.DisconnectAsync();
        _logger.LogInformation("Channel {Channel} stopped", Name);
    }

    private async Task PollingLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!_driver.IsConnected)
                {
                    await _driver.ConnectAsync();
                }

                // Read tags
                var readTags = _tags.Where(t => t.AccessRights.HasFlag(AccessRights.Read)).ToList();
                if (readTags.Any())
                {
                    var values = await _driver.ReadTagsAsync(readTags);

                    foreach (var kvp in values)
                    {
                        var tag = _tags.FirstOrDefault(t => t.Name == kvp.Key);
                        if (tag != null)
                        {
                            if (tag.Update(kvp.Value, TagQuality.Good))
                            {
                                _onTagChanged?.Invoke(tag);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Channel {Channel} polling loop error", Name);
                await Task.Delay(5000, token);
            }

            int scanRate = 100;
            if (_config.Options.TryGetValue("ScanRate", out var rateObj) && rateObj is int rate)
            {
                scanRate = rate;
            }
            else if (_config.Options.TryGetValue("ScanRate", out var rateStr) && int.TryParse(rateStr.ToString(), out var rateParsed))
            {
                scanRate = rateParsed;
            }

            if (scanRate > 0)
            {
                await Task.Delay(scanRate, token);
            }
        }
    }

    public async Task WriteTagAsync(string tagName, object value)
    {
        var tag = _tags.FirstOrDefault(t => t.Name == tagName);
        if (tag != null)
        {
            await _driver.WriteTagAsync(tag, value);
        }
    }

    public void Dispose()
    {
        StopAsync().Wait();
        _driver.Dispose();
    }
}
