using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Models;
using Plant01.Upper.Infrastructure.Configs.Models;
using Plant01.Upper.Infrastructure.Services;

namespace Plant01.Upper.Infrastructure.Workstations;

/// <summary>
/// 工位流程服务（监听触发标签，执行业务流程）
/// </summary>
public class WorkstationProcessService : IHostedService
{
    private readonly IDeviceCommunicationService _deviceComm;
    private readonly EquipmentConfigService _equipmentConfig;
    private readonly ProductionConfigManager _productionConfig;
    private readonly ILogger<WorkstationProcessService> _logger;
    
    // 工位流程处理器注册表
    private readonly Dictionary<string, IWorkstationProcessor> _processors = new();
    
    // 触发标签监听映射 (TagName -> TriggerInfo)
    private readonly Dictionary<string, WorkstationTriggerInfo> _triggerMappings = new();

    public WorkstationProcessService(
        IDeviceCommunicationService deviceComm,
        EquipmentConfigService equipmentConfig,
        ProductionConfigManager productionConfig,
        IEnumerable<IWorkstationProcessor> processors,
        ILogger<WorkstationProcessService> logger)
    {
        _deviceComm = deviceComm;
        _equipmentConfig = equipmentConfig;
        _productionConfig = productionConfig;
        _logger = logger;
        
        // 注册所有工位处理器
        foreach (var processor in processors)
        {
            _processors[processor.WorkstationType] = processor;
            _logger.LogDebug("注册工位处理器: {WorkstationType}", processor.WorkstationType);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在启动工位流程服务...");
        
        try
        {
            // 1. 构建触发标签映射
            BuildTriggerMappings();
            
            // 2. 订阅标签变化事件
            _deviceComm.TagChanged += OnTagChanged;
            
            _logger.LogInformation("工位流程服务已启动，监听 {Count} 个触发标签，注册 {ProcessorCount} 个工位处理器", 
                _triggerMappings.Count, _processors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "工位流程服务启动失败");
            throw;
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 构建触发标签映射关系
    /// </summary>
    private void BuildTriggerMappings()
    {
        // 从 equipment_mappings.csv 加载所有设备的标签映射
        var mappingsConfig = _equipmentConfig.GetAllMappings();
        
        foreach (var mappingDto in mappingsConfig)
        {
            var triggerMappings = mappingDto.TagMappings
                .Where(m => m.IsTrigger)
                .ToList();
            
            if (triggerMappings.Count == 0)
                continue;
            
            foreach (var mapping in triggerMappings)
            {
                _triggerMappings[mapping.TagName] = new WorkstationTriggerInfo
                {
                    EquipmentCode = mappingDto.EquipmentCode,
                    TagMapping = mapping,
                    LastTriggerTime = DateTime.MinValue
                };
                
                _logger.LogDebug("注册触发标签: {TagName} -> {Equipment} ({Purpose})",
                    mapping.TagName, mappingDto.EquipmentCode, mapping.Purpose);
            }
        }
    }

    /// <summary>
    /// 标签变化事件处理
    /// </summary>
    private async void OnTagChanged(object? sender, TagChangeEventArgs e)
    {
        // 检查是否为触发标签
        if (!_triggerMappings.TryGetValue(e.TagName, out var triggerInfo))
            return;

        // 检查触发条件
        if (!TriggerEvaluator.Evaluate(e.NewValue.Value, triggerInfo.TagMapping.TriggerCondition))
        {
            await WriteProcessResult(triggerInfo.EquipmentCode, ProcessResult.Idle);
            return;
        }

        // 防抖：避免重复触发（500ms内的重复触发会被忽略）
        var now = DateTime.Now;
        if ((now - triggerInfo.LastTriggerTime).TotalMilliseconds < 500)
        {
            _logger.LogTrace("触发信号防抖过滤: {TagName}", e.TagName);
            return;
        }
        
        triggerInfo.LastTriggerTime = now;

        _logger.LogInformation("检测到流程触发: {TagName} = {Value}, 设备: {Equipment}",
            e.TagName, e.NewValue.Value, triggerInfo.EquipmentCode);

        try
        {
            // 获取工位代码
            var workstation = _productionConfig.GetWorkstationByEquipment(triggerInfo.EquipmentCode);
            var workstationCode = workstation?.Code;
            var workstationType = workstation?.Type; // 假设 Workstation 实体有 Type 属性，或者通过命名规则推断

            if (string.IsNullOrEmpty(workstationCode))
            {
                _logger.LogWarning("设备 {EquipmentCode} 未关联工位", triggerInfo.EquipmentCode);
                return;
            }

            // 如果 Workstation 实体没有 Type 属性，这里暂时用硬编码
            if (string.IsNullOrEmpty(workstationType))
            {
                // 简单推断：如果工位代码包含 PKG 则为 Packaging
                if (workstationCode.Contains("PKG")) workstationType = "Packaging";
                else if (workstationCode.Contains("PAL")) workstationType = "Palletizing";
                else workstationType = "Unknown";
            }
            
            // 查找对应的工位处理器
            if (!_processors.TryGetValue(workstationType, out var processor))
            {
                _logger.LogWarning("未找到工位类型 {WorkstationType} 的流程处理器 (工位: {WorkstationCode})", workstationType, workstationCode);
                await WriteProcessResult(triggerInfo.EquipmentCode, ProcessResult.Error, "未找到工位处理器");
                return;
            }
            
            // 构建流程上下文
            var context = new WorkstationProcessContext
            {
                WorkstationCode = workstationCode,
                EquipmentCode = triggerInfo.EquipmentCode,
                TriggerTagName = e.TagName,
                TriggerValue = e.NewValue.Value,
                TriggerTime = now
            };
            
            // 执行工位流程
            await processor.ExecuteAsync(context);

            // 注意：Processor 内部已负责写回结果，此处不再重复写回，避免双重写入和潜在的配置错误
            await WriteProcessResult(triggerInfo.EquipmentCode, ProcessResult.Success);

            _logger.LogInformation("工位流程执行完成: {WorkstationCode}", workstationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行工位流程失败: {Equipment}", triggerInfo.EquipmentCode);

            // 注意：Processor 内部已负责写回错误结果
            await WriteProcessResult(triggerInfo.EquipmentCode, ProcessResult.Error, ex.Message);
        }
    }

    /// <summary>
    /// 写回流程结果到PLC
    /// </summary>
    private async Task WriteProcessResult(string equipmentCode, ProcessResult result, string? message = null)
    {
        try
        {
            var equipment = _equipmentConfig.GetEquipment(equipmentCode);
            if (equipment == null)
                return;
            
            // 查找 ProcessResult 用途的标签
            var resultMapping = equipment.TagMappings
                .FirstOrDefault(m => m.Purpose == TagPurpose.ProcessResult);
            
            if (resultMapping != null)
            {
                await _deviceComm.WriteTagAsync(resultMapping.TagName, (int)result);
                _logger.LogInformation("写回流程结果: {Equipment} -> {TagName} = {Result}", 
                    equipmentCode, resultMapping.TagName, result);
            }
            
            // 如果有消息标签，也写回消息
            if (!string.IsNullOrEmpty(message))
            {
                var messageMapping = equipment.TagMappings
                    .FirstOrDefault(m => m.TagName.Contains("Message") || m.TagName.Contains("Msg"));
                
                if (messageMapping != null)
                {
                    await _deviceComm.WriteTagAsync(messageMapping.TagName, message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写回流程结果失败: {Equipment}", equipmentCode);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("正在停止工位流程服务...");
        _deviceComm.TagChanged -= OnTagChanged;
        _logger.LogInformation("工位流程服务已停止");
        return Task.CompletedTask;
    }
}

/// <summary>
/// 工位触发信息
/// </summary>
internal class WorkstationTriggerInfo
{
    public string EquipmentCode { get; set; } = string.Empty;
    public TagMappingDto TagMapping { get; set; } = null!;
    public DateTime LastTriggerTime { get; set; }
}
