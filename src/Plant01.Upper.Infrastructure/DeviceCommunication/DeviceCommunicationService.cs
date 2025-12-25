using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models;
using Plant01.Upper.Infrastructure.DeviceCommunication.Configs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;
using Plant01.Upper.Infrastructure.DeviceCommunication.Engine;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication;

/// <summary>
/// 设备通信服务
/// </summary>
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

        _logger.LogInformation("[ 设备通信服务 ] 正在启动设备通信服务...");
        await ReloadAsync();
        _isRunning = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning) return;

        _logger.LogInformation("[ 设备通信服务 ] 正在停止设备通信服务...");
        await StopChannelsAsync();
        _isRunning = false;
    }

    private async void OnConfigChanged(object? sender, EventArgs e)
    {
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (Monitor.TryEnter(_reloadLock))
        {
            try
            {
                _logger.LogInformation("[ 设备通信服务 ] 正在重新加载配置...");

                // 1. 停止现有通道
                await StopChannelsAsync();

                // 2. 清除标签
                _tagEngine.Clear();

                // 3. 加载配置
                var channelConfigs = _configLoader.LoadChannels();
                var allTags = _configLoader.LoadTags();

                // 4. 初始化通道和设备
                foreach (var channelConfig in channelConfigs)
                {
                    if (!channelConfig.Enabled) continue;

                    try
                    {
                        // 为每个通道创建一个 Channel 实例
                        var channelLogger = _loggerFactory.CreateLogger<Channel>();
                        var channel = new Channel(
                            channelConfig,
                            driverType => _driverFactory.CreateDriver(driverType),
                            channelLogger,
                            _loggerFactory,
                            OnTagValueChanged);

                        // 为通道添加所有启用的设备
                        foreach (var deviceConfig in channelConfig.Devices)
                        {
                            if (!deviceConfig.Enabled) continue;

                            // 过滤此设备的标签
                            var deviceTags = allTags.Where(t =>
                                t.ChannelCode.Equals(channelConfig.Code, StringComparison.OrdinalIgnoreCase) &&
                                t.DeviceCode.Equals(deviceConfig.Name, StringComparison.OrdinalIgnoreCase)
                            ).ToList();

                            // 注册标签到引擎
                            foreach (var tag in deviceTags)
                            {
                                _tagEngine.RegisterTag(tag);
                            }

                            // 将设备添加到通道
                            channel.AddDevice(deviceConfig, deviceTags);
                            _logger.LogInformation("[ 设备通信服务 ] 已添加设备 {Device} 到通道 {Channel}，包含 {TagCount} 个标签",
                                deviceConfig.Name, channelConfig.Name, deviceTags.Count);
                        }

                        _channels.Add(channel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[ 设备通信服务 ] 初始化通道 {Channel} 失败", channelConfig.Name);
                    }
                }

                // 5. 启动通道
                foreach (var channel in _channels)
                {
                    channel.Start();
                }

                _logger.LogInformation("[ 设备通信服务 ] 重新加载完成。启动了 {Count} 个通道。", _channels.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ 设备通信服务 ] 重新加载失败。");
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

    private void OnTagValueChanged(CommunicationTag tag)
    {
        var snapshot = tag.GetSnapshot();
        var tagValue = new TagValue(tag.Name, snapshot.Value, (Domain.Models.TagQuality)snapshot.Quality, snapshot.Timestamp);
        TagChanged?.Invoke(this, new TagChangeEventArgs(tag.Name, tagValue));
    }

    public TagValue GetTagValue(string tagName)
    {
        var tag = _tagEngine.GetTag(tagName);
        if (tag == null)
        {
            return new TagValue(tagName, null, Domain.Models.TagQuality.Bad, DateTime.MinValue);
        }
        var snapshot = tag.GetSnapshot();
        return new TagValue(tag.Name, snapshot.Value, (Domain.Models.TagQuality)snapshot.Quality, snapshot.Timestamp);
    }

    public T GetTagValue<T>(string tagName, T defaultValue = default)
    {
        var data = GetTagValue(tagName);
        return data.GetValue<T>(defaultValue);
    }

    public async Task WriteTagAsync(string tagName, object value)
    {
        var tag = _tagEngine.GetTag(tagName);
        if (tag == null)
        {
            throw new KeyNotFoundException($"[ 设备通信服务 ] 标签 '{tagName}' 未找到。");
        }

        var channel = _channels.FirstOrDefault(c => c.Name.Equals(tag.ChannelCode, StringComparison.OrdinalIgnoreCase));
        if (channel == null)
        {
            throw new InvalidOperationException($"[ 设备通信服务 ] 标签 '{tagName}' 的通道 '{tag.ChannelCode}' 未找到。");
        }

        await channel.WriteTagAsync(tagName, value);
    }

    public void Dispose()
    {
        StopAsync().Wait();
        _hotReloader.Dispose();
    }
}
