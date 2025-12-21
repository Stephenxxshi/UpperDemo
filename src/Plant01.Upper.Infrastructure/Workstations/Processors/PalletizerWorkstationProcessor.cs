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
public class PalletizerWorkstationProcessor : IWorkstationProcessor
{
    public string WorkstationType => "Palletizer";

    private readonly IDeviceCommunicationService _deviceComm;
    private readonly IMesService _mesService;
    private readonly ILogger<PackagingWorkstationProcessor> _logger;
    private readonly IEquipmentConfigService _equipmentConfigService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PalletizerWorkstationProcessor(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        IEquipmentConfigService equipmentConfigService,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        IWorkOrderRepository workOrderRepository,
        ILogger<PackagingWorkstationProcessor> logger)
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
        _logger.LogInformation("[ {Workstation} ] 码垛工位触发流程: , 触发标签: [ {Tag} ]",
            context.WorkstationCode, context.TriggerTagName);

        try
        {

            // 获取设备配置以查找标签
            var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
            if (equipment == null)
            {
                _logger.LogError("[ {Code} ] : 未找到设备配置 ", context.EquipmentCode);
                return;
            }

            // 获取袋码
            var qrCodeTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagName.EndsWith(".Code"));
            string bagCode = string.Empty;

            if (qrCodeTag != null)
            {
                bagCode = _deviceComm.GetTagValue<string>(qrCodeTag.TagName);
            }

            if (string.IsNullOrEmpty(bagCode))
            {
                _logger.LogWarning("码垛机触发但未读取到袋码: {Code}", context.EquipmentCode);
                return;
            }

            // 查询码垛位置

            // 发送给PLC码垛位置

            // 保存袋码
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var bagRepo = unitOfWork.BagRepository;

            var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
            var bag = bags.FirstOrDefault();

            if (bag == null)
            {
                _logger.LogError($"未检测到袋码");
                return;
            }

            // 上站防错
            if (true)
            {
                bag.PalletizedAt = DateTime.UtcNow;
                bag.AddRecord(ProcessStep.Palletizing, context.EquipmentCode, true);
                await bagRepo.UpdateAsync(bag);

                await unitOfWork.SaveChangesAsync();
                _logger.LogInformation("袋码 [ {BagCode} ] 在 {MachineId} 码垛完成", bagCode, context.EquipmentCode);
            }

            _logger.LogInformation("码垛工位流程执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "码垛工位流程执行失败");
            throw;
        }
    }
}
