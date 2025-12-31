using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Services;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 码垛工位流程处理器示例
/// </summary>
public class PalletizerWorkStationProcessor : WorkstationProcessorBase
{
    public PalletizerWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger, ProductionConfigManager productionConfigManager) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger, productionConfigManager)
    {
        WorkstationType = "WS_Palletizer";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        // 获取包装实体


        // 查询码垛位置
        if (false)
        {
            // 上站防错

            // 查询码垛位置

            // 发送给PLC码垛位置
        }

        else
        {
            // 获取码盘数量及当前位置
            var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
            if (equipment == null)
            {
                _logger.LogError($"[ {context.EquipmentCode} ] >>> 袋码[ {bagCode} ] >>> 未找到设备配置 ");
                await WriteProcessResult(context, ProcessResult.Error, "没有找到设备配置");
                return;
            }

            // 获取机械臂当前位置及码盘数量
            var palletTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "PalletCode");
            if (palletTag is null)
            {
                _logger.LogError($"[ {context.EquipmentCode} ] >>> 袋码[ {bagCode} ] >>> 未找到 PalletCode 功能标签");
                await WriteProcessResult(context, ProcessResult.Error, "未找到 PalletCode 功能标签");
                return;
            }

            // 保存码垛信息
        }
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;

        var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
        var bag = bags.FirstOrDefault();

        if (bag == null)
        {
            _logger.LogError($"[ {context.EquipmentCode} ] >>> 袋码 [ {bagCode} ] >>> 未检测到袋码");
            return;
        }

        // 上站防错并保存袋码
        if (true)
        {
            bag.PalletizedAt = DateTime.UtcNow;
            bag.AddRecord(ProcessStep.Palletizing, context.EquipmentCode, true);
            await bagRepo.UpdateAsync(bag);

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"[ {context.EquipmentCode} ] >>> 袋码 [ {bagCode} ] >>> 码垛完成");
        }

        await WriteProcessResult(context, ProcessResult.Success);
        _logger.LogInformation($"[ {context.EquipmentCode} ] >>> 袋码 [ {bagCode} ] >>> 码垛工位流程执行完成");
    }

}
