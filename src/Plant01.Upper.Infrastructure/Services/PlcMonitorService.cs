using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Models;
using Plant01.Upper.Infrastructure.Configs.Models;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// PLC 监控服务
/// </summary>
public class PlcMonitorService : BackgroundService
{
    private readonly ILogger<PlcMonitorService> _logger;
    private readonly ITriggerDispatcher _dispatcher;
    private readonly IDeviceCommunicationService _deviceService;
    private readonly EquipmentConfigService _configService;

    // 标签配置缓存: TagName -> (EquipmentCode, Mapping)
    private Dictionary<string, (string EquipmentCode, TagMappingDto Mapping)> _tagConfigCache = new();

    public PlcMonitorService(
        ILogger<PlcMonitorService> logger, 
        ITriggerDispatcher dispatcher,
        IDeviceCommunicationService deviceService,
        EquipmentConfigService configService)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _deviceService = deviceService;
        _configService = configService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC 监控服务已启动，正在监听事件模式...");
        
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
                    // 假设 TagName 是唯一的，如果有重复，后加载的覆盖
                    _tagConfigCache[mapping.TagName] = (equipment.EquipmentCode, mapping);
                }
            }
            _logger.LogInformation("已构建标签配置缓存，共 {Count} 个标签", _tagConfigCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "构建标签配置缓存失败");
        }
    }

    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        try
        {
            // 1. 查找配置
            if (!_tagConfigCache.TryGetValue(e.TagName, out var config))
            {
                // 未配置的标签，忽略
                return;
            }

            var (equipmentCode, mapping) = config;

            // 2. 检查是否为触发器
            // 兼容 Purpose == "ProcessTrigger" 或 IsTrigger == true
            bool isTrigger = mapping.IsTrigger || 
                             string.Equals(mapping.Purpose, "ProcessTrigger", StringComparison.OrdinalIgnoreCase);

            if (!isTrigger)
            {
                return;
            }

            // 3. 检查触发条件
            if (CheckTriggerCondition(e.NewValue.Value, mapping.TriggerCondition))
            {
                _logger.LogInformation("触发器激活: {Equipment} - {Tag} (Value: {Value})", 
                    equipmentCode, e.TagName, e.NewValue.Value);

                // 4. 发送调度消息
                // 使用 EquipmentCode 作为 StationId
                await _dispatcher.EnqueueAsync(
                    stationId: equipmentCode,
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

    /// <summary>
    /// 检查值是否满足触发条件
    /// </summary>
    private bool CheckTriggerCondition(object? value, string? condition)
    {
        // 如果没有条件，默认 bool true 触发，或者非 null 触发
        if (string.IsNullOrWhiteSpace(condition))
        {
            if (value is bool bVal) return bVal;
            // 对于数字，非0触发？暂时保守一点，只处理bool
            return value != null;
        }

        condition = condition.Trim();

        // 处理 "=value" 格式
        if (condition.StartsWith("=") || condition.StartsWith("=="))
        {
            var targetStr = condition.TrimStart('=').Trim();
            
            // Bool 比较
            if (bool.TryParse(targetStr, out var bTarget))
            {
                // 尝试将 value 转为 bool
                if (value is bool bValue) return bValue == bTarget;
                if (value != null && bool.TryParse(value.ToString(), out var bValueConv)) return bValueConv == bTarget;
            }

            // 数字比较
            if (double.TryParse(targetStr, out var dTarget) && ConvertToDouble(value, out var dValue))
            {
                return Math.Abs(dValue - dTarget) < 0.0001;
            }

            // 字符串比较
            return string.Equals(value?.ToString(), targetStr, StringComparison.OrdinalIgnoreCase);
        }

        // 可以在此扩展 >, <, != 等逻辑
        
        return false;
    }

    private bool ConvertToDouble(object? value, out double result)
    {
        result = 0;
        if (value == null) return false;
        
        try
        {
            result = Convert.ToDouble(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override void Dispose()
    {
        _deviceService.TagChanged -= OnTagChanged;
        base.Dispose();
    }
}
