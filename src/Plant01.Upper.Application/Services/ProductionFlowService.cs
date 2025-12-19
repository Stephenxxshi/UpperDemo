using CommunityToolkit.Mvvm.Messaging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Messages;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Services;

public class ProductionFlowService : IPlcFlowService, IRecipient<StationTriggerMessage>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductionFlowService> _logger;

    public ProductionFlowService(IServiceScopeFactory scopeFactory, ILogger<ProductionFlowService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

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
        // 假设 Payload 就是 BagCode，或者包含更多信息
        // 实际项目中可能需要解析 JSON Payload
        string bagCode = message.Payload;
        string machineId = message.StationId; // 简化处理，直接用 StationId 作为 MachineId

        switch (message.StationId)
        {
            case "ST01_Loading": // 上袋
                await ProcessLoadingRequestAsync(bagCode, machineId, serviceProvider);
                break;
            case "ST02_Bagging": // 套袋
                await ProcessBaggingRequestAsync(bagCode, machineId, serviceProvider);
                break;
            case "ST03_Filling": // 灌装
                await ProcessFillingRequestAsync(bagCode, machineId, serviceProvider);
                break;
            case "ST04_Weighing": // 复检称重
                // 假设 Payload 格式为 "BagCode:Weight" 或者只是 BagCode 然后去读 PLC
                // 这里简化假设 Payload 包含重量信息，例如 "BAG123:50.5"
                var parts = bagCode.Split(':');
                if (parts.Length == 2 && double.TryParse(parts[1], out double weight))
                {
                    await ProcessWeighingRequestAsync(parts[0], machineId, weight, serviceProvider);
                }
                else
                {
                    _logger.LogWarning("称重无效负载：{Payload}", message.Payload);
                }
                break;
            case "ST05_Printing": // 喷码
                await ProcessPrintingRequestAsync(bagCode, machineId, serviceProvider);
                break;
            case "ST06_Palletizing": // 码垛
                // 假设 Payload 为 "BagCode:PalletCode:Position"
                // 简化：只传 BagCode，PalletCode 和 Position 需要另外获取或管理
                await ProcessPalletizingRequestAsync(bagCode, "UNKNOWN_PALLET", machineId, 0, serviceProvider);
                break;
            case "ST07_PalletOut": // 出垛
                await ProcessPalletOutRequestAsync(bagCode, machineId, serviceProvider); // 这里 bagCode 可能是 PalletCode
                break;
            default:
                _logger.LogWarning("未知站点：{StationId}", message.StationId);
                break;
        }
    }

    // --- 原有业务逻辑 (保留并适配) ---

    public async Task<bool> ProcessLoadingRequestAsync(string bagCode, string machineId, IServiceProvider? serviceProvider = null)
    {
        var unitOfWork = serviceProvider?.GetRequiredService<IUnitOfWork>() ?? _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

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
        var bagRepo = unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

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
        var bagRepo = unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

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
        var bagRepo = unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

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
        var bagRepo = unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

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
        var bagRepo = unitOfWork.Repository<Bag>();
        var palletRepo = unitOfWork.Repository<Pallet>();

        var bag = await bagRepo.GetByIdAsync(bagCode);
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
