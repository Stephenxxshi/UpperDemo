using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 工位流程处理器基类
/// </summary>
public abstract class PalletStackWorkStationProcessor : IWorkstationProcessor
{
    public string WorkstationType { get; } = "WS_PalletStack";
    private string WorkStationProcess = "进母托盘垛工位流程";
    private Equipment TriggerEquipment;
    private readonly IDeviceCommunicationService _deviceComm;
    private readonly IMesService _mesService;
    private readonly ILogger<PalletStackWorkStationProcessor> _logger;
    private readonly IEquipmentConfigService _equipmentConfigService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ProductionConfigManager _productionConfig;

    public PalletStackWorkStationProcessor(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        IEquipmentConfigService equipmentConfigService,
        ProductionConfigManager productionConfig,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        ILogger<PalletStackWorkStationProcessor> logger)
    {
        _deviceComm = deviceComm;
        _mesService = mesService;
        _logger = logger;
        _equipmentConfigService = equipmentConfigService;
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _productionConfig = productionConfig;
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

        // 获取是母托盘还是子托盘缺少
        int palletType = 1;

        // 获取AGV代码
        var line = _productionConfig.GetProductionLineByWorkstation(context.WorkstationCode);
        if (line is null)
        {
            _logger.LogError("[ {WorkStationProcess} ] 未找到生产线配置，无法获取AGV代码", WorkStationProcess);
            await WriteProcessResult(context, ProcessResult.Error, "未找到生产线配置");
            return;
        }
        var agvCode = line.AgvDeviceCode;

        // 发送缺托盘信息给MES
        LackPalletRequest request = new LackPalletRequest
        {
            AgvDeviceCode = agvCode,
            PalletType = palletType
        };

        // todo:写一个通用的将报文保存到指定文件夹的方法
        var mesApiResponse = await _mesService.ReportLackPalletAsync(request);
        if (!mesApiResponse.IsSuccess)
        {
            _logger.LogError("[ {WorkStationProcess} ] MES接口调用失败 {mesApiResponse}", WorkStationProcess, mesApiResponse.ErrorMsg);
            await WriteProcessResult(context, ProcessResult.Error, mesApiResponse.ErrorMsg);
            return;
        }

        await WriteProcessResult(context, ProcessResult.Success, "缺托盘信息已发送到MES");
    }

    protected async Task<Equipment?> GetEquipmentMent(WorkstationProcessContext context)
    {
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        if (equipment == null)
        {
            _logger.LogError($"[ {WorkStationProcess} ]  [ {context.TriggerTagName} ] -> 在工位  [ {context.WorkstationCode} ] : 未找到设备配置 ");

            await WriteProcessResult(context, ProcessResult.Error, "未找到设备配置");
        }
        return equipment;
    }

    /// <summary>
    /// 写回流程结果到PLC
    /// </summary>
    protected async Task WriteProcessResult(WorkstationProcessContext context, ProcessResult result, string? message = null)
    {
        var equipment = await GetEquipmentMent(context);
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
                    //_logger.LogInformation($"[ {WorkStationProcess} ] {context.BagCode ?? string.Empty} ] -> 写入 {messageMapping?.TagCode} = {(int)result}");
                }
            }
            // 查找 ProcessResult 用途的标签
            var resultMapping = equipment.TagMappings
                .FirstOrDefault(m => m.Purpose == TagPurpose.ProcessResult);

            if (resultMapping != null)
            {
                await _deviceComm.WriteTagAsync(resultMapping.TagCode, (int)result);
                //_logger.LogInformation($"[ {WorkStationProcess} ]  [ {context.BagCode ?? string.Empty} ] -> 写入 [ {resultMapping.TagCode} ] ：{(int)result}({result})");
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
