using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// PLC 监控服务
/// </summary>
public class PlcMonitorService : BackgroundService
{
    private readonly ILogger<PlcMonitorService> _logger;
    private readonly ITriggerDispatcher _dispatcher;
    private readonly IDeviceCommunicationService _deviceService;

    public PlcMonitorService(
        ILogger<PlcMonitorService> logger, 
        ITriggerDispatcher dispatcher,
        IDeviceCommunicationService deviceService)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _deviceService = deviceService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC 监控服务已启动（桥接模式）。");
        
        // 订阅标签变化事件
        _deviceService.TagChanged += OnTagChanged;
        
        return Task.CompletedTask;
    }

    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        // 如需要，仅过滤相关标签
        // 目前，我们假设任何标签变化都是触发器或数据更新
        
        // 示例映射逻辑：
        // 如果标签名称以 "ST" 开头，它可能是站点触发器
        // 例如 "ST01_Loading.Trigger"
        
        try
        {
            // 简单逻辑：目前只是转发一切，或按特定标签过滤
            // 在真实应用中，您会在 "TriggerMap" 中查找标签
            
            // 示例：如果值为布尔 TRUE，则触发事件
            if (e.NewData.Value is bool bVal && bVal)
            {
                // 从 TagName 中提取 StationId（例如 "SDJ01.HeartBreak" -> "SDJ01"）
                var parts = e.TagName.Split('.');
                var stationId = parts.Length > 0 ? parts[0] : "Unknown";
                
                await _dispatcher.EnqueueAsync(
                    stationId: stationId,
                    source: TriggerSourceType.PLC,
                    payload: $"{e.TagName}={e.NewData.Value}",
                    priority: TriggerPriority.Normal,
                    debounceKey: e.TagName // 按标签名称去抖
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理标签变化时出错 {Tag}", e.TagName);
        }
    }

    public override void Dispose()
    {
        _deviceService.TagChanged -= OnTagChanged;
        base.Dispose();
    }
}
