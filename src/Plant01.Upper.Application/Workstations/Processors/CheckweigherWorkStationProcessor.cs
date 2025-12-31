using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

using System.Text.Json;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 出垛工位流程处理器示例
/// </summary>
public class CheckweigherWorkStationProcessor : WorkstationProcessorBase
{
    public CheckweigherWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger, ProductionConfigManager productionConfigManager) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger, productionConfigManager)
    {
        WorkstationType = "WS_Checkweigher";
        WorkStationProcess = "复检称重流程";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        // 获取实际重量
        var actualWeightTag = equipment.TagMappings.FirstOrDefault(tag => tag.Purpose == "Data");
        double? actualWeight = null;
        if (actualWeightTag != null)
        {
            actualWeight = _deviceComm.GetTagValue<float>(actualWeightTag.TagCode);
        }

        // 判断重量是否合格

        // 保存袋码
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;

        var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
        var bag = bags.FirstOrDefault();
        if (bag is null)
        {
            _logger.LogWarning($"[ {WorkStationProcess} ] 袋码 : [ {bagCode} ] -> 未找到对应的袋码信息");
            await WriteProcessResult(context, ProcessResult.Error, "未找到对应的袋信息");
            return;
        }

        // 是否可以称重
        if (!bag.CanWeigh())
        {
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 前道工序未完成");
            await WriteProcessResult(context, ProcessResult.Error, "前道工序未完成");
            return;
        }

        // 更新实际重量
        var obj = new
        {
            ActualWeight = actualWeight
        };

        string data = JsonSerializer.Serialize(obj);
        bag.AddRecord(ProcessStep.Weighing, context.EquipmentCode, true, data);

        // 更新袋码状态
        await unitOfWork.SaveChangesAsync();

        // 发送PLC
        await WriteProcessResult(context, ProcessResult.Success, "复检称重流程完成");
        _logger.LogInformation($"[ {WorkStationProcess} ] 袋码 : [ {bagCode} ] -> 复检称重工位流程执行完成");
    }


}
