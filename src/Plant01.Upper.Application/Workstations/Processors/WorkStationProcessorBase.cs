using Plant01.Domain.Shared.Models.Equipment;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 工位流程处理器基类
/// </summary>
public abstract class WorkstationProcessorBase : IWorkstationProcessor
{
    public string WorkstationType { get; protected set; } = string.Empty;
    protected string WorkStationProcess = string.Empty;
    protected Equipment TriggerEquipment;

    protected readonly IDeviceCommunicationService _deviceComm;
    protected readonly IMesService _mesService;
    protected readonly ILogger<WorkstationProcessorBase> _logger;
    protected readonly IEquipmentConfigService _equipmentConfigService;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IWorkOrderRepository _workOrderRepository;
    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly ProductionConfigManager _productionConfig;

    public WorkstationProcessorBase(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        IEquipmentConfigService equipmentConfigService,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        IWorkOrderRepository workOrderRepository,
        ILogger<WorkstationProcessorBase> logger,
        ProductionConfigManager productionConfigManager)
    {
        _deviceComm = deviceComm;
        _mesService = mesService;
        _logger = logger;
        _workOrderRepository = workOrderRepository;
        _equipmentConfigService = equipmentConfigService;
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _productionConfig = productionConfigManager;
    }

    public async Task ExecuteAsync(WorkstationProcessContext context)
    {
        // 获取产线实体
        var productLine = _productionConfig.GetProductionLineByEquipment(context.EquipmentCode);
        var lineEquipment = productLine.Workstations.SelectMany(x => x.Equipments)
            .FirstOrDefault(x => x.Capabilities == Capabilities.LineStatus);

        // PLC是否手动模式
        if (lineEquipment is null)
        {
            _logger.LogError($"[ {context.WorkstationCode} ] >>> [ {context.TriggerTagName} ]: 未找到设备 {context.EquipmentCode}");
            await WriteProcessResult(context, ProcessResult.Error, $"未找到设备 {context.EquipmentCode}");
            return;
        }
        var manualModeTag = lineEquipment.TagMappings.FirstOrDefault(m => m.Purpose == "ManualMode");
        var autoModeTag = lineEquipment.TagMappings.FirstOrDefault(m => m.Purpose == "AutoMode");
        if (manualModeTag != null && autoModeTag != null)
        {
            var isManualMode = _deviceComm.GetTagValue<bool>(manualModeTag.TagCode);
            var isAutoMode = _deviceComm.GetTagValue<bool>(autoModeTag.TagCode);
            if (isManualMode && !isAutoMode)
            {
                _logger.LogInformation($"[ {context.WorkstationCode} ] 设备处于手动模式，跳过流程执行");
                return;
            }
        }

        _logger.LogInformation($"[ {context.WorkstationCode} ] [ 标签: {context.TriggerTagName} ] 触发流程", WorkStationProcess, context.TriggerTagName);

        // 获取设备配置以查找标签
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        if (equipment == null)
        {
            _logger.LogError($"[ {context.WorkstationCode} ]  [ {context.TriggerTagName} ] -> 在工位  [ {context.WorkstationCode} ] : 未找到设备配置 ");

            await WriteProcessResult(context, ProcessResult.Error, "未找到设备配置");
            return;
        }


        // 获取袋码标签
        var qrCodeTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagCode.EndsWith(".Code"));
        string bagCode = string.Empty;

        if (qrCodeTag is null)
        {
            _logger.LogWarning($"[ {context.WorkstationCode} ]  [ {context.TriggerTagName} ] 触发但未找到QrCode");
            await WriteProcessResult(context, ProcessResult.Error, "未找到QrCode标签");
            return;
        }

        // 获取袋码
        _logger.LogDebug($"[ {context.WorkstationCode} ]  [ {context.TriggerTagName} ] 读取袋码标签: {qrCodeTag.TagCode}");
        bagCode = _deviceComm.GetTagValue<string>(qrCodeTag.TagCode);

        if (string.IsNullOrEmpty(bagCode))
        {
            _logger.LogWarning($"[ {context.WorkstationCode} ]  [ {context.TriggerTagName} ] 触发但未读取到袋码: {bagCode}");
            await WriteProcessResult(context, ProcessResult.Error, "未读取到袋码");
            return;
        }

        // 通过袋码获取数据库实体

        await InternalExecuteAsync(context, bagCode);
    }

    /// <summary>
    /// 写回流程结果到PLC
    /// </summary>
    protected async Task WriteProcessResult(WorkstationProcessContext context, ProcessResult result, string? message = null)
    {
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode)!;
        try
        {
            // 如果有消息标签，写回消息
            if (!string.IsNullOrEmpty(message))
            {
                var messageMapping = equipment.TagMappings
                    .FirstOrDefault(m => m.TagCode.Contains("Message") || m.TagCode.Contains("Msg"));

                if (messageMapping != null)
                {
                    await _deviceComm.WriteTagAsync(messageMapping.TagCode, message);
                    //_logger.LogInformation($"[ {context.WorkstationCode} ] {context.BagCode ?? string.Empty} ] -> 写入 {messageMapping?.TagCode} = {(int)result}");
                }
            }
            // 查找 ProcessResult 用途的标签
            var resultMapping = equipment.TagMappings
                .FirstOrDefault(m => m.Purpose == TagPurpose.ProcessResult);

            if (resultMapping != null)
            {
                await _deviceComm.WriteTagAsync(resultMapping.TagCode, (int)result);
                //_logger.LogInformation($"[ {context.WorkstationCode} ]  [ {context.BagCode ?? string.Empty} ] -> 写入 [ {resultMapping.TagCode} ] ：{(int)result}({result})");
            }


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写回流程结果失败: {Equipment}", equipment.Code);
        }
    }

    protected virtual async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        return;
    }
}
