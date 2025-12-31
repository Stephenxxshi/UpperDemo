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
public class InkjetWorkStationProcessor : WorkstationProcessorBase
{
    public InkjetWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger, ProductionConfigManager productionConfigManager) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger, productionConfigManager)
    {
        WorkstationType = "WS_Inkjet";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {

        // 获取喷码内容
        string inkjetContent = "test"; // TODO: 从配置或其他来源获取喷码内容

        // 发送喷码内容



        // 保存袋码
        // 查询袋码实体
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;

        var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
        if (bags.Count > 1)
        {
            _logger.LogError($"[ {context.EquipmentCode} ] >>> 袋码 : [ {bagCode} ] -> 存在多个相同的袋码信息");
        }

        var bag = bags.FirstOrDefault();
        if (bag is null)
        {
            _logger.LogWarning($"[ {context.EquipmentCode} ] >>> 袋码 : [ {bagCode} ] -> 未找到对应的袋码信息");
            await WriteProcessResult(context, ProcessResult.Error, "未找到对应的袋信息");
            return;
        }

        // 是否可以喷码
        if (!bag.CanInkjet())
        {
            _logger.LogError($"[ {context.EquipmentCode} ] >>> 袋码[ {bagCode} ] -> 前道工序未完成");
            await WriteProcessResult(context, ProcessResult.Error, "前道工序未完成");
            return;
        }

        // 更新发送内容
        var obj = new
        {
            InkjetContent = inkjetContent
        };

        string data = JsonSerializer.Serialize(obj);
        bag.AddRecord(ProcessStep.Inkjet, context.EquipmentCode, true, data);

        // 更新袋码状态
        int result = await unitOfWork.SaveChangesAsync();
        if (result <= 0)
        {
            _logger.LogError($"[ {context.EquipmentCode} ] >>> 袋码[ {bagCode} ] -> 更新失败");
            await WriteProcessResult(context, ProcessResult.Error, "更新袋码失败");
            return;
        }
        else
        {
            // 发送PLC
            await WriteProcessResult(context, ProcessResult.Success, "喷码成功");
            _logger.LogInformation($"[ {context.EquipmentCode} ] >>> 袋码 [ {bagCode} ] >>> 完成喷码");
        }

    }


}
