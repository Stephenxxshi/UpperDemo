using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
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

    protected readonly IDeviceCommunicationService _deviceComm;
    protected readonly IMesService _mesService;
    protected readonly ILogger<WorkstationProcessorBase> _logger;
    protected readonly IEquipmentConfigService _equipmentConfigService;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IWorkOrderRepository _workOrderRepository;
    protected readonly IServiceScopeFactory _serviceScopeFactory;

    public WorkstationProcessorBase(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        IEquipmentConfigService equipmentConfigService,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        IWorkOrderRepository workOrderRepository,
        ILogger<WorkstationProcessorBase> logger)
    {
        _deviceComm = deviceComm;
        _mesService = mesService;
        _logger = logger;
        _workOrderRepository = workOrderRepository;
        _equipmentConfigService = equipmentConfigService;
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ExecuteAsync(WorkstationProcessContext context)
    {
        // PLC是否手动模式
        var mdj1 = _equipmentConfigService.GetEquipment("MDJ1");
        var manualModeTag = mdj1?.TagMappings.FirstOrDefault(m => m.Purpose == "ManualMode");
        var autoModeTag = mdj1?.TagMappings.FirstOrDefault(m => m.Purpose == "AutoMode");
        if (manualModeTag != null && autoModeTag != null)
        {
            var isManualMode = _deviceComm.GetTagValue<bool>(manualModeTag.TagCode);
            var isAutoMode = _deviceComm.GetTagValue<bool>(autoModeTag.TagCode);
            if (isManualMode && !isAutoMode)
            {
                _logger.LogInformation("[ {WorkStationProcess} ] 设备处于手动模式，跳过流程执行", WorkStationProcess);
                return;
            }
        }

        _logger.LogInformation("[ {WorkStationProcess} ] [ 标签: {Tag} ] 触发流程", WorkStationProcess, context.TriggerTagName);

        // 获取设备配置以查找标签
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        if (equipment == null)
        {
            _logger.LogError($"[ {WorkStationProcess} ]  [ {context.TriggerTagName} ] -> 在工位  [ {context.WorkstationCode} ] : 未找到设备配置 ");

            await WriteProcessResult(context, ProcessResult.Error, "未找到设备配置");
            return;
        }

        context.Equipment = equipment;

        // 获取袋码标签
        var qrCodeTag = context.Equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagCode.EndsWith(".Code"));
        string bagCode = string.Empty;

        if (qrCodeTag != null)
        {
            bagCode = _deviceComm.GetTagValue<string>(qrCodeTag.TagCode);
        }

        if (string.IsNullOrEmpty(bagCode))
        {
            _logger.LogWarning($"[ {WorkStationProcess} ]  [ {context.TriggerTagName} ] 触发但未读取到袋码: {bagCode}");
            await WriteProcessResult(context, ProcessResult.Error, "未读取到袋码");
            return;
        }

        context.BagCode = bagCode;

        await InternalExecuteAsync(context, bagCode);


    }

    /// <summary>
    /// 写回流程结果到PLC
    /// </summary>
    protected async Task WriteProcessResult(WorkstationProcessContext context, ProcessResult result, string? message = null)
    {
        try
        {
            // 如果有消息标签，写回消息
            if (!string.IsNullOrEmpty(message))
            {
                var messageMapping = context.Equipment.TagMappings
                    .FirstOrDefault(m => m.TagCode.Contains("Message") || m.TagCode.Contains("Msg"));

                if (messageMapping != null)
                {
                    await _deviceComm.WriteTagAsync(messageMapping.TagCode, message);
                    _logger.LogInformation($"[ {WorkStationProcess} ] {context.BagCode ?? string.Empty} ] -> 写入 {messageMapping?.TagCode} = {(int)result}");
                }
            }
            // 查找 ProcessResult 用途的标签
            var resultMapping = context.Equipment.TagMappings
                .FirstOrDefault(m => m.Purpose == TagPurpose.ProcessResult);

            if (resultMapping != null)
            {
                await _deviceComm.WriteTagAsync(resultMapping.TagCode, (int)result);
                _logger.LogInformation($"[ {WorkStationProcess} ]  [ {context.BagCode ?? string.Empty} ] -> 写入 [ {resultMapping.TagCode} ] ：{(int)result}({result})");
            }


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写回流程结果失败: {Equipment}", context.Equipment.Code);
        }
    }

    protected virtual async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        return;
    }
}
