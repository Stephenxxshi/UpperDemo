using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.Configs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;
using Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

namespace Plant01.Upper.Infrastructure.DeviceCommunication;

public class DeviceCommunicationService : IDeviceCommunicationService, IHostedService, IDisposable
{
    private readonly ConfigurationLoader _configLoader;
    private readonly ConfigHotReloader _hotReloader;
    private readonly DriverFactory _driverFactory;
    private readonly TagEngine _tagEngine;
    private readonly ILogger<DeviceCommunicationService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    private readonly List<Channel> _channels = new();
    private readonly object _reloadLock = new();
    private bool _isRunning;

    public event EventHandler<TagChangeEventArgs>? TagChanged;

    public DeviceCommunicationService(
        ConfigurationLoader configLoader,
        ConfigHotReloader hotReloader,
        DriverFactory driverFactory,
        TagEngine tagEngine,
        ILogger<DeviceCommunicationService> logger,
        ILoggerFactory loggerFactory)
    {
        _configLoader = configLoader;
        _hotReloader = hotReloader;
        _driverFactory = driverFactory;
        _tagEngine = tagEngine;
        _logger = logger;
        _loggerFactory = loggerFactory;

        _hotReloader.ConfigChanged += OnConfigChanged;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;
        
        _logger.LogInformation("启动设备通信服务...");
        await ReloadAsync();
        _isRunning = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning) return;

        _logger.LogInformation("停止设备通信服务...");
        await StopChannelsAsync();
        _isRunning = false;
    }

    private async void OnConfigChanged(object? sender, EventArgs e)
    {
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        // 简单锁以防止并发重新加载
        // 在实际应用中，使用 SemaphoreSlim 进行异步锁定
        if (Monitor.TryEnter(_reloadLock))
        {
            try
            {
                _logger.LogInformation("重新加载配置...");

                // 1. 停止现有通道
                await StopChannelsAsync();

                // 2. 清除引擎
                _tagEngine.Clear();

                // 3. 加载新配置
                var channelConfigs = _configLoader.LoadChannels();
                var allTags = _configLoader.LoadTags();

                // 4. 重新建立通道
                foreach (var config in channelConfigs)
                {
                    // 为此通道/驱动程序过滤标签
                    // 假设通道名称或驱动代码匹配。
                    // CSV 中有 "DriverCode"（例如 PLC01）。通道 JSON 中有 "Name"（例如 PLC01）。
                    var channelTags = allTags.Where(t => t.DriverCode.Equals(config.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    // 将标签注册到引擎
                    foreach (var tag in channelTags)
                    {
                        _tagEngine.RegisterTag(tag);
                    }

                    // 创建驱动
                    var driver = _driverFactory.CreateDriver(config.Driver);

                    // 创建通道
                    var channelLogger = _loggerFactory.CreateLogger<Channel>();
                    var channel = new Channel(config, driver, channelTags, channelLogger, OnTagValueChanged);
                    
                    _channels.Add(channel);
                }

                // 5. 启动通道
                foreach (var channel in _channels)
                {
                    channel.Start();
                }

                _logger.LogInformation("重新加载完成。已启动 {Count} 个通道。", _channels.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新加载配置失败。");
            }
            finally
            {
                Monitor.Exit(_reloadLock);
            }
        }
    }

    private async Task StopChannelsAsync()
    {
        foreach (var channel in _channels)
        {
            await channel.StopAsync();
            channel.Dispose();
        }
        _channels.Clear();
    }

    private void OnTagValueChanged(Tag tag)
    {
        // 触发事件
        TagChanged?.Invoke(this, new TagChangeEventArgs(tag.Name, tag.GetSnapshot()));
    }

    public TagData GetTagValue(string tagName)
    {
        var tag = _tagEngine.GetTag(tagName);
        if (tag == null)
        {
            return new TagData(null, TagQuality.Bad, DateTime.MinValue);
        }
        return tag.GetSnapshot();
    }

    public async Task WriteTagAsync(string tagName, object value)
    {
        var tag = _tagEngine.GetTag(tagName);
        if (tag == null)
        {
            throw new KeyNotFoundException($"找不到标签 '{tagName}'.");
        }

        var channel = _channels.FirstOrDefault(c => c.Name == tag.DriverCode);
        if (channel == null)
        {
             throw new InvalidOperationException($"找不到标签 '{tagName}' 对应的通道.");
        }

        await channel.WriteTagAsync(tagName, value);
    }

    public void Dispose()
    {
        StopAsync().Wait();
        _hotReloader.Dispose();
    }
}
