using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Infrastructure.Workstations.Processors;

/// <summary>
/// 码垛工位流程处理器示例
/// </summary>
public class PalletizerWorkstationProcessor : WorkstationProcessorBase
{
    public PalletizerWorkstationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "Palletizer";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        // 查询码垛位置

        // 发送给PLC码垛位置

        // 查询代码
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;

        var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
        var bag = bags.FirstOrDefault();

        if (bag == null)
        {
            _logger.LogError($"袋码 [ {bagCode} ] 在 {context.EquipmentCode}未检测到袋码");
            return;
        }

        // 上站防错并保存袋码
        if (true)
        {
            bag.PalletizedAt = DateTime.UtcNow;
            bag.AddRecord(ProcessStep.Palletizing, context.EquipmentCode, true);
            await bagRepo.UpdateAsync(bag);

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"袋码 [ {bagCode} ] 在 {context.EquipmentCode} 码垛完成");
        }

        await WriteProcessResult(context,ProcessResult.Success);
        _logger.LogInformation($"袋码 [ {bagCode} ] 在 {context.EquipmentCode} 码垛工位流程执行完成");
    }

}
