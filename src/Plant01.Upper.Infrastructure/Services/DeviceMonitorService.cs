using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Core.Helper;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Messages;
using Plant01.Upper.Application.Models;
using Plant01.Upper.Application.Services;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 设备监控服务 (DeviceMonitorService)
/// 职责：
/// 1. 监听底层设备通信服务的标签变化事件 (支持 PLC, Modbus, OPC UA 等所有驱动)
/// 2. 将标签变化映射到具体的设备 (Equipment)
/// 3. 对于触发器标签：通过 TriggerDispatcher 发送业务触发消息
/// 4. 对于普通标签：通过 Messenger 发送状态变化消息 (供 UI 或状态服务使用)
/// </summary>
public class DeviceMonitorService : BackgroundService
{
    private readonly ILogger<DeviceMonitorService> _logger;
    private readonly ITriggerDispatcher _dispatcher;
    private readonly IDeviceCommunicationService _deviceService;
    private readonly EquipmentConfigService _configService;
    private readonly IMessenger _messenger;

    // 标签配置缓存: TagName -> (EquipmentCode, Mapping)
    private Dictionary<string, (string EquipmentCode, TagMappingDto Mapping)> _tagConfigCache = new();

    public DeviceMonitorService(
        ILogger<DeviceMonitorService> logger, 
        ITriggerDispatcher dispatcher,
        IDeviceCommunicationService deviceService,
        EquipmentConfigService configService,
        IMessenger messenger)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _deviceService = deviceService;
        _configService = configService;
        _messenger = messenger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ 设备监控服务 ] 已启动，正在监听事件模式...");
        
        // 构建配置缓存
        BuildTagConfigCache();

        // 订阅标签变化事件
        _deviceService.TagChanged += OnTagChanged;
        
        return Task.CompletedTask;
    }

    private void BuildTagConfigCache()
    {
        try
        {
            var allMappings = _configService.GetAllMappings();
            _tagConfigCache.Clear();
            foreach (var equipment in allMappings)
            {
                foreach (var mapping in equipment.TagMappings)
                {
                    // TagCode 必须是唯一的，如果有重复，后加载的覆盖
                    _tagConfigCache[mapping.TagCode] = (equipment.EquipmentCode, mapping);
                }
            }
            _logger.LogInformation("[ 设备监控服务 ] 已构建标签配置缓存，共 {Count} 个标签", _tagConfigCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ 设备监控服务 ] 构建标签配置缓存失败");
        }
    }

    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        try
        {
            // 1. 查找配置
            if (!_tagConfigCache.TryGetValue(e.TagCode, out var config))
            {
                // 未配置的标签，忽略
                _logger.LogWarning($"[ 设备监控服务 ] 未配置的标签:{e.TagCode}");
                return;
            }

            var (equipmentCode, mapping) = config;
            var transformedValue = TagValueTransformEvaluator.EvaluateOrFallback(mapping, e.NewValue.Value, _logger);

            // 2. 发送通用状态变化消息 (给 UI 或 状态服务)
            // 无论是否为触发器，只要值变化了，都应该通知上层
            _messenger.Send(new TagValueChangedMessage(
                EquipmentCode: equipmentCode,
                TagCode: e.TagCode,
                NewValue: e.NewValue.Value,
                OldValue: null, // 事件参数中暂无旧值
                Purpose: mapping.Purpose,
                Timestamp: DateTime.Now
            ));

            // 3. 检查是否为触发器
            // 兼容 Purpose == "ProcessTrigger" 或 IsTrigger == true
            bool isTrigger = mapping.IsTrigger || 
                             string.Equals(mapping.Purpose, "ProcessTrigger", StringComparison.OrdinalIgnoreCase);

            if (!isTrigger)
            {
                return;
            }

            // 4. 检查触发条件
            if (TriggerEvaluator.Evaluate(transformedValue, mapping.TriggerCondition))
            {
                _logger.LogInformation("[ 设备监控服务 ] 触发器激活: {Equipment} - {Tag} (Value: {Value})", 
                    equipmentCode, e.TagCode, transformedValue);

                // 5. 发送调度消息
                // 使用 EquipmentCode 作为 StationId
                await _dispatcher.EnqueueAsync(
                    stationId: equipmentCode,
                    source: e.TriggerSourceType,
                    payload: $"{e.TagCode}={transformedValue}",
                    priority: TriggerPriority.Normal,
                    debounceKey: e.TagCode // 按标签名防抖
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ 设备监控服务 ] 处理标签变化时出错 {Tag}", e.TagCode);
        }
    }

    public override void Dispose()
    {
        _deviceService.TagChanged -= OnTagChanged;
        base.Dispose();
    }
}
