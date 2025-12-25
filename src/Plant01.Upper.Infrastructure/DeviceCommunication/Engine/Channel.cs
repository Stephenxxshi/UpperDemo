using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

/// <summary>
/// 设备通信通道类，负责管理一个驱动类型下多个设备的通信连接和数据轮询
/// </summary>
public class Channel : IDisposable
{
    private readonly ChannelConfig _channelConfig;
    private readonly string _driverType;
    private readonly List<DeviceConnection> _deviceConnections = new();
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Func<string, IDriver> _driverFactory;
    private readonly Action<CommunicationTag>? _onTagChanged;

    /// <summary>
    /// 获取通道名称
    /// </summary>
    public string Name => _channelConfig.Code;

    /// <summary>
    /// 初始化通道
    /// </summary>
    /// <param name="channelConfig">通道配置</param>
    /// <param name="driverFactory">驱动工厂委托</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="loggerFactory">日志工厂</param>
    /// <param name="onTagChanged">标签变化回调</param>
    public Channel(
        ChannelConfig channelConfig,
        Func<string, IDriver> driverFactory,
        ILogger logger,
        ILoggerFactory loggerFactory,
        Action<CommunicationTag>? onTagChanged = null)
    {
        _channelConfig = channelConfig;
        _driverType = channelConfig.DriverType;
        _driverFactory = driverFactory;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _onTagChanged = onTagChanged;
    }

    /// <summary>
    /// 添加设备连接
    /// </summary>
    public void AddDevice(DeviceConfig deviceConfig, IEnumerable<CommunicationTag> tags)
    {
        var driver = _driverFactory(_driverType);
        if (driver == null)
        {
            _logger.LogError("无法创建驱动类型: {DriverType}", _driverType);
            return;
        }
        var deviceLogger = _loggerFactory.CreateLogger<DeviceConnection>();
        var deviceConnection = new DeviceConnection(deviceConfig, driver, tags, deviceLogger, _onTagChanged);
        _deviceConnections.Add(deviceConnection);
    }

    /// <summary>
    /// 启动通道（启动所有设备连接）
    /// </summary>
    public void Start()
    {
        if (!_channelConfig.Enabled) return;

        foreach (var deviceConnection in _deviceConnections)
        {
            deviceConnection.Start();
        }
        _logger.LogInformation("通道 {Channel} 已启动，共 {Count} 个设备", Name, _deviceConnections.Count);
    }

    /// <summary>
    /// 异步停止通道（停止所有设备连接）
    /// </summary>
    public async Task StopAsync()
    {
        var stopTasks = _deviceConnections.Select(dc => dc.StopAsync());
        await Task.WhenAll(stopTasks);
        _logger.LogInformation("通道 {Channel} 已停止", Name);
    }

    /// <summary>
    /// 异步写入标签值
    /// </summary>
    /// <param name="tagName">标签名称</param>
    /// <param name="value">要写入的值</param>
    public async Task WriteTagAsync(string tagName, object value)
    {
        // 查找包含该标签的设备连接
        foreach (var deviceConnection in _deviceConnections)
        {
            if (await deviceConnection.TryWriteTagAsync(tagName, value))
            {
                return;
            }
        }
        throw new KeyNotFoundException($"标签 '{tagName}' 未在通道 '{Name}' 的任何设备中找到。");
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopAsync().Wait();
        foreach (var deviceConnection in _deviceConnections)
        {
            deviceConnection.Dispose();
        }
        _deviceConnections.Clear();
    }

    /// <summary>
    /// 设备连接类，管理单个设备的通信和轮询
    /// </summary>
    private class DeviceConnection : IDisposable
    {
        private readonly DeviceConfig _config;
        private readonly IDriver _driver;
        private readonly List<CommunicationTag> _tags = new();
        private readonly ILogger _logger;
        private readonly Action<CommunicationTag>? _onTagChanged;
        private CancellationTokenSource? _cts;
        private Task? _pollingTask;

        public string Name => _config.Name;

        public DeviceConnection(
            DeviceConfig config,
            IDriver driver,
            IEnumerable<CommunicationTag> tags,
            ILogger logger,
            Action<CommunicationTag>? onTagChanged = null)
        {
            _config = config;
            _driver = driver;
            _tags.AddRange(tags);
            _logger = logger;
            _onTagChanged = onTagChanged;

            // 初始化并验证驱动
            if (_driver != null)
            {
                _driver.Initialize(_config);
                _driver.ValidateConfig(_config);
            }
            else
            {
                _logger.LogError("驱动实例为空，无法初始化设备 {Device}", config.Name);
            }
        }

        public void Start()
        {
            if (!_config.Enabled) return;

            _cts = new CancellationTokenSource();
            _pollingTask = Task.Run(() => PollingLoop(_cts.Token));
            _logger.LogInformation("设备 {Device} 已启动", Name);
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
            _logger.LogInformation("设备 {Device} 已停止", Name);
        }

        private async Task PollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 检查连接状态，如果未连接则尝试连接
                    if (!_driver.IsConnected)
                    {
                        await _driver.ConnectAsync();
                    }

                    // 读取标签
                    var readTags = _tags.Where(t => t.AccessRights.HasFlag(Models.AccessRights.Read)).ToList();
                    if (readTags.Any())
                    {
                        var values = await _driver.ReadTagsAsync(readTags);

                        foreach (var kvp in values)
                        {
                            var tag = _tags.FirstOrDefault(t => t.Name == kvp.Key);
                            if (tag != null)
                            {
                                // 记录更新前的状态，用于判断是否是首次初始化
                                bool isFirstLoad = tag.CurrentQuality == Models.TagQuality.Bad && tag.CurrentTimestamp == DateTime.MinValue;

                                // 如果标签值发生变化，触发回调
                                if (tag.Update(kvp.Value, Models.TagQuality.Good))
                                {
                                    // 注意：首次从 Bad->Good 的变化用于 UI 初始化。
                                    // 如果业务逻辑（如报警）不希望在启动时触发，可以取消下行的注释并使用 if (!isFirstLoad)
                                    // if (!isFirstLoad) 
                                    Task.Run(() =>
                                    {
                                        _logger.LogDebug("设备 {Device} 标签 {Tag} 值已更新为 {Value}", Name, tag.Name, kvp.Value);
                                        _onTagChanged?.Invoke(tag);
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "设备 {Device} 轮询循环错误", Name);
                    await Task.Delay(5000, token); // 错误后等待5秒重试
                }

                // 获取扫描速率
                int scanRate = 100;
                if (_config.Options.TryGetValue("ScanRate", out var rateObj) && rateObj is int rate)
                {
                    scanRate = rate;
                }
                else if (_config.Options.TryGetValue("ScanRate", out var rateStr) && int.TryParse(rateStr.ToString(), out var rateParsed))
                {
                    scanRate = rateParsed;
                }

                // 等待扫描间隔
                if (scanRate > 0)
                {
                    await Task.Delay(scanRate, token);
                }
            }
        }

        public async Task<bool> TryWriteTagAsync(string tagName, object value)
        {
            var tag = _tags.FirstOrDefault(t => t.Name == tagName);
            if (tag != null)
            {
                await _driver.WriteTagAsync(tag, value);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _driver.Dispose();
        }
    }
}
