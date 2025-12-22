using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Infrastructure.Workstations.Processors;

/// <summary>
/// 包装工位流程处理器示例
/// </summary>
public class PackagingWorkstationProcessor : WorkstationProcessorBase
{

    public PackagingWorkstationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "Packaging";
    }


    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        // 读取工单号
        var workOrders = await _workOrderRepository.GetAllAsync(workOrder => workOrder.Status == Domain.ValueObjects.WorkOrderStatus.开工);
        if (workOrders.Count == 0)
        {
            _logger.LogWarning($"袋码[ {bagCode} ] -> 没有找到开工中的工单");
            await WriteProcessResult(context, ProcessResult.Error, "没有找到开工中的工单");
            return;
        }

        if (workOrders.Count > 1)
        {
            _logger.LogError($"袋码[ {bagCode} ] -> 开工的工单数量为 [ {workOrders.Count} ] > 1");
            await WriteProcessResult(context, ProcessResult.Error, "开工的工单数量异常");
            return;
        }

        var currentWorkOrder = workOrders.First();

        // 获取设备配置以查找标签
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        if (equipment == null)
        {
            _logger.LogError($"袋码[ {bagCode} ] -> 未找到设备配置: {context.EquipmentCode}");
            await WriteProcessResult(context, ProcessResult.Error, "没有找到设备配置");
            return;
        }


        // 保存袋码
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;

        var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
        var bag = bags.FirstOrDefault();
        bool isNew = false;

        if (bag == null)
        {
            // 上袋是第一步，如果不存在则创建
            bag = new Bag
            {
                BagCode = bagCode,
                CreatedAt = DateTime.Now,
                OrderCode = currentWorkOrder.Code,
                ProductCode = currentWorkOrder.ProductCode,
                ProductAlias = currentWorkOrder.ProductName,
                LineNo = currentWorkOrder.LineNo,
                StationNo = context.WorkstationCode,
                ProductWeightUnit = "kg", // 默认单位
                ProductHeightUnit = "mm",  // 默认单位
                BatchCode = currentWorkOrder.BatchNumber
            };
            await bagRepo.AddAsync(bag);
            isNew = true;
        }


        //if (bag.CanLoad())
        if (true)
        {
            bag.AddRecord(ProcessStep.Loading, context.EquipmentCode, true);

            if (!isNew)
            {
                await bagRepo.UpdateAsync(bag);
            }

            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"袋码[ {bagCode} ] -> 在 {context.EquipmentCode} 加载");
        }

        await WriteProcessResult(context, ProcessResult.Success, "包装工位流程执行完成");
        _logger.LogInformation($"袋码[ {bagCode} ] -> 在 {context.EquipmentCode} 包装工位流程执行完成");
    }


}
