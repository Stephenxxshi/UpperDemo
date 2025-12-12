using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

public class Channel : IDisposable
{
    private readonly ChannelConfig _config;
    private readonly IDriver _driver;
    private readonly List<Tag> _tags = new();
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private Task? _pollingTask;

    public string Name => _config.Name;

    private readonly Action<Tag>? _onTagChanged;

    public Channel(ChannelConfig config, IDriver driver, IEnumerable<Tag> tags, ILogger logger, Action<Tag>? onTagChanged = null)
    {
        _config = config;
        _driver = driver;
        _tags.AddRange(tags);
        _logger = logger;
        _onTagChanged = onTagChanged;
    }

    public void Start()
    {
        if (!_config.Enable) return;

        _cts = new CancellationTokenSource();
        _pollingTask = Task.Run(() => PollingLoop(_cts.Token));
        _logger.LogInformation("通道 {Channel} 已启动。", Name);
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
        _logger.LogInformation("通道 {Channel} 已停止。", Name);
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

                // 读取标签
                // 优化：在实际实现中，这里按内存区域分组标签
                var readTags = _tags.Where(t => !t.IsWriteOnly).ToList();
                if (readTags.Any())
                {
                    var values = await _driver.ReadTagsAsync(readTags);

                    foreach (var kvp in values)
                    {
                        var tag = _tags.FirstOrDefault(t => t.Name == kvp.Key);
                        if (tag != null)
                        {
                            // 线程安全更新
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
                _logger.LogError(ex, "通道 {Channel} 轮询循环出错。", Name);
                // 等待一段时间后重试连接
                await Task.Delay(5000, token);
            }

            if (_config.ScanRate > 0)
            {
                await Task.Delay(_config.ScanRate, token);
            }
        }
    }

    public async Task WriteTagAsync(string tagName, object value)
    {
        var tag = _tags.FirstOrDefault(t => t.Name == tagName);
        if (tag != null)
        {
            await _driver.WriteTagAsync(tag, value);
            // 乐观地更新本地标签？还是等待下次轮询？
            // 通常等待下次轮询或立即读回。
            // 对于模拟，让我们更新本地。
            // 可选
            // tag.Update(value, TagQuality.Good); // Optional
        }
    }

    public void Dispose()
    {
        StopAsync().Wait();
        _driver.Dispose();
    }
}
