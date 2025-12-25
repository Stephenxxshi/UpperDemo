using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Infrastructure.Services;

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
        _logger.LogInformation("心跳服务已启动");

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

        _logger.LogInformation("心跳服务已停止");
    }

    private async Task ProcessHeartbeatsAsync()
    {
        // 获取所有设备
        var equipments = _configManager.GetAllProductionLines()
            .SelectMany(l => l.Workstations)
            .SelectMany(w => w.Equipments);

        foreach (var equipment in equipments)
        {
            if (!equipment.Enabled) continue;

            // 筛选 Output 且 Purpose 为 Heartbeat 的标签
            var heartbeatTags = equipment.TagMappings
                .Where(m => m.Purpose == TagPurpose.Heartbeat && m.Direction == TagDirection.Output);

            foreach (var mapping in heartbeatTags)
            {
                try
                {
                    // 简单的布尔翻转逻辑
                    if (!_heartbeatStates.ContainsKey(mapping.TagName))
                    {
                        _heartbeatStates[mapping.TagName] = false;
                    }

                    // 翻转状态
                    _heartbeatStates[mapping.TagName] = !_heartbeatStates[mapping.TagName];
                    var valueToWrite = _heartbeatStates[mapping.TagName];

                    // 写入标签
                    await _deviceCommunicationService.WriteTagAsync(mapping.TagName, valueToWrite);
                    
                    // _logger.LogTrace("已发送心跳至 {Tag}: {Value}", mapping.TagName, valueToWrite);
                }
                catch (Exception ex)
                {
                    // 仅记录警告，避免刷屏
                    _logger.LogWarning("向标签 {Tag} 写入心跳失败: {Message}", mapping.TagName, ex.Message);
                }
            }
        }
    }
}
