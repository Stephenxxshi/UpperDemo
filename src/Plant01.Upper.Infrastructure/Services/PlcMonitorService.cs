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
        _logger.LogInformation("PLC 监控服务已启动，正在监听事件模式...");
        
        // 订阅标签变化事件
        _deviceService.TagChanged += OnTagChanged;
        
        return Task.CompletedTask;
    }

    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        // 这里不需要过滤特定标签
        // 目前策略是监听任何标签变化，由调度器进行数据过滤
        
        // 示例映射逻辑：
        // 如果标签名以 "ST" 开头，则作为站点触发器
        // 例如 "ST01_Loading.Trigger"
        
        try
        {
            // 此逻辑目前只做转发，不处理特定标签逻辑
            // 在真实应用中，可能在 "TriggerMap" 中查找标签
            
            // 示例：当值为 TRUE 时触发事件
            if (e.NewValue.Value is bool bVal && bVal)
            {
                // 从 TagName 中提取 StationId（例如 "SDJ01.HeartBreak" -> "SDJ01"）
                var parts = e.TagName.Split('.');
                var stationId = parts.Length > 0 ? parts[0] : "Unknown";
                
                await _dispatcher.EnqueueAsync(
                    stationId: stationId,
                    source: TriggerSourceType.PLC,
                    payload: $"{e.TagName}={e.NewValue.Value}",
                    priority: TriggerPriority.Normal,
                    debounceKey: e.TagName // 按标签名防抖
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
