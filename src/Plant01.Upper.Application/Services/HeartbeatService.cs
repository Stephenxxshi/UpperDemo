using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Application.Services;

/// <summary>
/// 心跳服务
/// 负责向PLC发送心跳信号（Output Heartbeat）
/// </summary>
public class HeartbeatService : BackgroundService
{
    private readonly IDeviceCommunicationService _deviceCommunicationService;
    private readonly ProductionConfigManager _configManager;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly Dictionary<string, bool> _heartbeatStates = new();

    // 优化：缓存心跳标签列表，避免每秒重复筛选
    private List<EquipmentTagMapping> _cachedHeartbeatTags = new();

    public HeartbeatService(
        IDeviceCommunicationService deviceCommunicationService,
        ProductionConfigManager configManager,
        ILogger<HeartbeatService> logger)
    {
        _deviceCommunicationService = deviceCommunicationService;
        _configManager = configManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ 心跳服务 ] 心跳服务已启动");

        // 启动时初始化缓存
        RefreshHeartbeatTags();

        // 1秒心跳周期
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessHeartbeatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "心跳处理循环发生异常");
            }
        }

        _logger.LogInformation("[ 心跳服务 ] 心跳服务已停止");
    }

    /// <summary>
    /// 刷新心跳标签缓存
    /// (如果将来实现了配置热更新，可在配置变更事件中调用此方法)
    /// </summary>
    public void RefreshHeartbeatTags()
    {
        var tags = new List<EquipmentTagMapping>();

        var equipments = _configManager.GetAllProductionLines()
            .SelectMany(l => l.Workstations)
            .SelectMany(w => w.Equipments);

        foreach (var equipment in equipments)
        {
            if (!equipment.Enabled) continue;

            var heartbeatTags = equipment.TagMappings
                .Where(m => m.Purpose == TagPurpose.Heartbeat && m.Direction == TagDirection.Output);

            tags.AddRange(heartbeatTags);
        }

        _cachedHeartbeatTags = tags;
        _logger.LogInformation("[ 心跳服务 ] 心跳标签缓存已更新，共监控 {Count} 个输出心跳", _cachedHeartbeatTags.Count);
    }

    private async Task ProcessHeartbeatsAsync()
    {
        // 优化：直接遍历缓存列表，性能极高
        foreach (var mapping in _cachedHeartbeatTags)
        {
            try
            {
                // 初始化状态
                if (!_heartbeatStates.ContainsKey(mapping.TagCode))
                {
                    _heartbeatStates[mapping.TagCode] = false;
                }

                // 翻转状态 (0 -> 1 -> 0)
                _heartbeatStates[mapping.TagCode] = !_heartbeatStates[mapping.TagCode];
                var valueToWrite = _heartbeatStates[mapping.TagCode];

                // 写入标签
                await _deviceCommunicationService.WriteTagAsync(mapping.TagCode, valueToWrite);
            }
            catch (Exception ex)
            {
                // 仅记录警告，避免刷屏
                _logger.LogWarning("[ 心跳服务 ] 向标签 {Tag} 写入心跳失败: {Message}", mapping.TagCode, ex.Message);
            }
        }
    }
}
