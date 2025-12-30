using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

using System.Text.Json;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 包装工位流程处理器示例
/// </summary>
public class PackagingWorkstationProcessor : WorkstationProcessorBase
{

    public PackagingWorkstationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "WS_Packaging";
        WorkStationProcess = "包装工位流程";
    }


    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        // 读取工单号
        var workOrders = await _workOrderRepository.GetAllAsync(workOrder => workOrder.Status == Domain.ValueObjects.WorkOrderStatus.开工);
        if (workOrders.Count == 0)
        {
            _logger.LogWarning($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 没有找到开工中的工单");
            await WriteProcessResult(context, ProcessResult.Error, "没有找到开工中的工单");
            return;
        }

        if (workOrders.Count > 1)
        {
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 开工的工单数量为 [ {workOrders.Count} ] > 1");
            await WriteProcessResult(context, ProcessResult.Error, "开工的工单数量异常");
            return;
        }

        var currentWorkOrder = workOrders.First();

        // 获取设备配置以查找标签
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        if (equipment == null)
        {
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 未找到设备配置: {context.EquipmentCode}");
            await WriteProcessResult(context, ProcessResult.Error, "没有找到设备配置");
            return;
        }

        // 获取包装时间和包装重量
        var packagingWeightTag = equipment.TagMappings.FirstOrDefault(tag => tag.Purpose == "Weight");
        var packagingTimeSpanTag = equipment.TagMappings.FirstOrDefault(tag => tag.Purpose == "TimeOffset");
        float? packagingWeight = null;
        float? packagingTimeSpan = null;
        if (packagingWeightTag != null && packagingTimeSpanTag != null)
        {
            packagingWeight = _deviceComm.GetTagValue<float>(packagingWeightTag.TagCode);
            packagingTimeSpan = _deviceComm.GetTagValue<float>(packagingTimeSpanTag.TagCode);
        }


        // 获取袋子实体
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;

        var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
        var bag = bags.FirstOrDefault();
        bool isNew = false;

        if (bag == null)
        {
            _logger.LogInformation($"[ {WorkStationProcess} ] 袋码:[ {bagCode} ] -> 创建新袋码记录");
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
                BatchCode = currentWorkOrder.BatchNumber,
                PackagingWeight = packagingWeight,
                PackagingTimeSpan = TimeSpan.FromSeconds((double)packagingTimeSpan)
            };
            await bagRepo.AddAsync(bag);
            isNew = true;
        }

        // 是否可以包装
        if (!bag.CanPackaging())
        {
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 袋码已使用，无法再次包装");
            await WriteProcessResult(context, ProcessResult.Error, "袋码已使用，无法再次包装");
            return;
        }

        // 增加包装记录
        var obj = new
        {
            PackagingWeight = packagingWeight,
            PackagingTimeSpan = packagingTimeSpan
        };
        string data = JsonSerializer.Serialize(obj);
        bag.AddRecord(ProcessStep.Packaging, context.EquipmentCode, true, data);

        if (!isNew)
        {
            await bagRepo.UpdateAsync(bag);
        }

        // 更新袋码状态
        await unitOfWork.SaveChangesAsync();
        _logger.LogInformation($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 在 {context.EquipmentCode} 包装");
        await WriteProcessResult(context, ProcessResult.Success, "包装工位流程执行完成");
        _logger.LogInformation($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] 流程执行完成");


    }


}
