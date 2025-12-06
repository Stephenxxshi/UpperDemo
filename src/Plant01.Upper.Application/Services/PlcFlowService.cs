using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Services;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Services;

public class PlcFlowService : IPlcFlowService
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly ILogger<PlcFlowService> _logger;

    public PlcFlowService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ProcessLoadingRequestAsync(string bagCode, string machineId)
    {
        var bagRepo = _unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

        if (bag == null)
        {
            // 上袋是第一步，如果不存在则创建
            // 注意：这里需要知道工单号，通常PLC会传工单号或者PC当前激活的工单
            // 假设这里简化处理，或者需要从其他地方获取当前工单
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
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> ProcessBaggingRequestAsync(string bagCode, string machineId)
    {
        var bagRepo = _unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

        if (bag != null && bag.CanBag())
        {
            bag.AddRecord(ProcessStep.Bagging, machineId, true);
            await bagRepo.UpdateAsync(bag);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> ProcessFillingRequestAsync(string bagCode, string machineId)
    {
        var bagRepo = _unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

        if (bag != null && bag.CanFill())
        {
            bag.AddRecord(ProcessStep.Filling, machineId, true);
            await bagRepo.UpdateAsync(bag);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> ProcessWeighingRequestAsync(string bagCode, string machineId, double weight)
    {
        var bagRepo = _unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

        if (bag != null && bag.CanWeigh())
        {
            // 这里应该获取工单的标准重量进行比对
            // var workOrder = await _unitOfWork.Repository<WorkOrder>().GetByIdAsync(bag.OrderCode);
            // bool isQualified = Math.Abs(weight - workOrder.StandardWeight) < tolerance;
            bool isQualified = true; // 暂时默认合格

            bag.AddRecord(ProcessStep.Weighing, machineId, isQualified, weight.ToString());
            bag.ProductActualWeight = (float)weight; // 更新袋子属性

            await bagRepo.UpdateAsync(bag);
            await _unitOfWork.SaveChangesAsync();
            return isQualified;
        }
        return false;
    }

    public async Task<string?> ProcessPrintingRequestAsync(string bagCode, string machineId)
    {
        var bagRepo = _unitOfWork.Repository<Bag>();
        var bag = await bagRepo.GetByIdAsync(bagCode);

        if (bag != null && bag.CanPrint())
        {
            // 获取喷码内容，通常来自工单
            string printContent = $"CODE:{bagCode}"; // 示例

            bag.AddRecord(ProcessStep.Printing, machineId, true, printContent);
            await bagRepo.UpdateAsync(bag);
            await _unitOfWork.SaveChangesAsync();
            return printContent;
        }
        return null;
    }

    public async Task<bool> ProcessPalletizingRequestAsync(string bagCode, string palletCode, string machineId, int positionIndex)
    {
        var bagRepo = _unitOfWork.Repository<Bag>();
        var palletRepo = _unitOfWork.Repository<Pallet>();
        
        var bag = await bagRepo.GetByIdAsync(bagCode);
        var pallet = await palletRepo.GetByIdAsync(palletCode);

        if (bag != null && bag.CanPalletize())
        {
            if (pallet == null)
            {
                pallet = new Pallet { PalletCode = palletCode, WorkOrderCode = bag.OrderCode };
                await palletRepo.AddAsync(pallet);
            }

            // 更新袋子状态
            bag.AddRecord(ProcessStep.Palletizing, machineId, true, $"Pallet:{palletCode}");
            bag.PalletCode = palletCode;
            bag.LoadPosition = (ushort)positionIndex;
            bag.PalletizedAt = DateTime.Now;

            // 更新托盘状态
            pallet.AddBag(bagCode, positionIndex);
            pallet.CurrentPalletizerId = machineId;

            await bagRepo.UpdateAsync(bag);
            await palletRepo.UpdateAsync(pallet);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> ProcessPalletOutRequestAsync(string palletCode, string machineId)
    {
        var palletRepo = _unitOfWork.Repository<Pallet>();
        var pallet = await palletRepo.GetByIdAsync(palletCode);

        if (pallet != null && !pallet.OutTime.HasValue)
        {
            // 检查是否满垛?
            // if (pallet.Items.Count >= StandardCount) ...

            pallet.OutTime = DateTime.Now;
            pallet.IsFull = true; // 假设出垛即满垛
            
            // 记录出垛事件? 可以在 Pallet 聚合里加 Record 或者单独的 Event
            
            await palletRepo.UpdateAsync(pallet);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
