using CommunityToolkit.Mvvm.Messaging;

using Plant01.Domain.Shared.Models.Equipment;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Messages;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Services;

public class ProductionFlowService : IPlcFlowService, IRecipient<StationTriggerMessage>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductionFlowService> _logger;
    private readonly IEquipmentConfigService _equipmentConfigService;
    private readonly IDeviceCommunicationService _deviceService;

    public ProductionFlowService(
        IServiceScopeFactory scopeFactory,
        ILogger<ProductionFlowService> logger,
        IEquipmentConfigService equipmentConfigService,
        IDeviceCommunicationService deviceService)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _equipmentConfigService = equipmentConfigService;
        _deviceService = deviceService;

        // 注册消息监听
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(StationTriggerMessage message)
    {
        _logger.LogInformation("收到处理触发：{StationId}", message.StationId);

        // 使用 Task.Run 避免阻塞 Messenger 分发线程 (虽然 Dispatcher 已经在后台，但为了保险)
        Task.Run(async () =>
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    await HandleTriggerAsync(message, scope.ServiceProvider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理 {StationId} 触发时出错", message.StationId);
            }
        });
    }

    private async Task HandleTriggerAsync(StationTriggerMessage message, IServiceProvider serviceProvider)
    {
        var equipmentCode = message.StationId;
        var equipment = _equipmentConfigService.GetEquipment(equipmentCode);

        if (equipment == null)
        {
            _logger.LogWarning("未找到设备配置：{EquipmentCode}", equipmentCode);
            return;
        }

        _logger.LogInformation("处理设备触发: {Code} ({Type})", equipment.Code, equipment.Type);

        switch (equipment.Type)
        {
            case EquipmentType.BagPicker:
                await HandleBagPickerTrigger(equipment, message, serviceProvider);
                break;
            case EquipmentType.Palletizer:
                await HandlePalletizerTrigger(equipment, message, serviceProvider);
                break;
            // 可以继续扩展其他类型
            default:
                _logger.LogWarning("未处理的设备类型：{Type}", equipment.Type);
                break;
        }
    }

    private async Task HandleBagPickerTrigger(Equipment equipment, StationTriggerMessage message, IServiceProvider serviceProvider)
    {
        // 尝试获取二维码
        var qrCodeTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagName.EndsWith(".Code"));
        string bagCode = string.Empty;

        if (qrCodeTag != null)
        {
            bagCode = _deviceService.GetTagValue<string>(qrCodeTag.TagName);
        }

        if (string.IsNullOrEmpty(bagCode))
        {
            // 如果没读到，尝试从 Payload 解析 (如果 Payload 包含)
            // 或者生成一个临时 Code
            bagCode = $"AUTO_{DateTime.Now:yyyyMMddHHmmss}_{equipment.Code}";
            _logger.LogWarning("未读取到二维码，使用生成码: {Code}", bagCode);
        }

        await ProcessLoadingRequestAsync(bagCode, equipment.Code, serviceProvider);
    }

    private async Task HandlePalletizerTrigger(Equipment equipment, StationTriggerMessage message, IServiceProvider serviceProvider)
    {
        // 尝试获取二维码 (可能是袋码)
        var qrCodeTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagName.EndsWith(".Code"));
        string bagCode = string.Empty;

        if (qrCodeTag != null)
        {
            bagCode = _deviceService.GetTagValue<string>(qrCodeTag.TagName);
        }

        if (string.IsNullOrEmpty(bagCode))
        {
            _logger.LogWarning("码垛机触发但未读取到袋码: {Code}", equipment.Code);
            return;
        }

        // 还需要 PalletCode 和 Position
        // 这里简化处理，假设 PalletCode 是当前正在使用的托盘
        // 实际逻辑可能需要查询当前托盘状态
        string palletCode = $"PAL_{DateTime.Now:yyyyMMdd}"; // 临时
        int position = 1; // 临时

        await ProcessPalletizingRequestAsync(bagCode, palletCode, equipment.Code, position, serviceProvider);
    }

    // --- 原有业务逻辑 (保留并适配) ---

    public async Task<bool> ProcessLoadingRequestAsync(string bagCode, string machineId, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var bag = await bagRepo.GetByCodeAsync(bagCode);

        if (bag == null)
        {
            // 上袋是第一步，如果不存在则创建
            bag = new Bag
            {
                BagCode = bagCode,
                CreatedAt = DateTime.Now,
                // OrderCode = currentWorkOrderCode // 需要获取当前工单
            };
            await bagRepo.AddAsync(bag);
        }

        if (bag.CanLoad())
        {
            bag.AddRecord(ProcessStep.Loading, machineId, true);
            await bagRepo.UpdateAsync(bag);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("袋 {BagCode} 在 {MachineId} 加载", bagCode, machineId);
            return true;
        }

        return false;
    }

    public async Task<bool> ProcessBaggingRequestAsync(string bagCode, string machineId, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var bag = await bagRepo.GetByCodeAsync(bagCode);

        if (bag != null && bag.CanBag())
        {
            bag.AddRecord(ProcessStep.Bagging, machineId, true);
            await bagRepo.UpdateAsync(bag);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("袋 {BagCode} 在 {MachineId} 套袋", bagCode, machineId);
            return true;
        }
        return false;
    }

    public async Task<bool> ProcessFillingRequestAsync(string bagCode, string machineId, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var bag = await bagRepo.GetByCodeAsync(bagCode);

        if (bag != null && bag.CanFill())
        {
            bag.AddRecord(ProcessStep.Filling, machineId, true);
            await bagRepo.UpdateAsync(bag);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("袋 {BagCode} 在 {MachineId} 灌装", bagCode, machineId);
            return true;
        }
        return false;
    }

    public async Task<bool> ProcessWeighingRequestAsync(string bagCode, string machineId, double weight, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var bag = await bagRepo.GetByCodeAsync(bagCode);

        if (bag != null && bag.CanWeigh())
        {
            bool isQualified = true; // 暂时默认合格

            bag.AddRecord(ProcessStep.Weighing, machineId, isQualified, weight.ToString());
            bag.ProductActualWeight = (float)weight;

            await bagRepo.UpdateAsync(bag);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("袋 {BagCode} 在 {MachineId} 称重：{Weight}", bagCode, machineId, weight);
            return isQualified;
        }
        return false;
    }

    public async Task<string?> ProcessPrintingRequestAsync(string bagCode, string machineId, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var bag = await bagRepo.GetByCodeAsync(bagCode);

        if (bag != null && bag.CanPrint())
        {
            string printContent = $"CODE:{bagCode}";

            bag.AddRecord(ProcessStep.Printing, machineId, true, printContent);
            await bagRepo.UpdateAsync(bag);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("袋 {BagCode} 在 {MachineId} 喷码", bagCode, machineId);
            return printContent;
        }
        return null;
    }

    public async Task<bool> ProcessPalletizingRequestAsync(string bagCode, string palletCode, string machineId, int positionIndex, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var palletRepo = unitOfWork.Repository<Pallet>();

        var bag = await bagRepo.GetByCodeAsync(bagCode);
        var pallet = await palletRepo.GetByIdAsync(palletCode);

        if (bag != null && bag.CanPalletize())
        {
            if (pallet == null)
            {
                pallet = new Pallet { PalletCode = palletCode, WorkOrderCode = bag.OrderCode };
                await palletRepo.AddAsync(pallet);
            }

            bag.AddRecord(ProcessStep.Palletizing, machineId, true, $"Pallet:{palletCode}");
            bag.PalletCode = palletCode;
            bag.LoadPosition = (ushort)positionIndex;
            bag.PalletizedAt = DateTime.Now;

            pallet.AddBag(bagCode, positionIndex);
            pallet.CurrentPalletizerId = machineId;

            await bagRepo.UpdateAsync(bag);
            await palletRepo.UpdateAsync(pallet);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("袋 {BagCode} 码垛到 {PalletCode}", bagCode, palletCode);
            return true;
        }
        return false;
    }

    public async Task<bool> ProcessPalletOutRequestAsync(string palletCode, string machineId, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var palletRepo = unitOfWork.Repository<Pallet>();
        var pallet = await palletRepo.GetByIdAsync(palletCode);

        if (pallet != null && !pallet.OutTime.HasValue)
        {
            pallet.OutTime = DateTime.Now;
            pallet.IsFull = true;

            await palletRepo.UpdateAsync(pallet);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("托盘 {PalletCode} 在 {MachineId} 出垛", palletCode, machineId);
            return true;
        }
        return false;
    }
}
