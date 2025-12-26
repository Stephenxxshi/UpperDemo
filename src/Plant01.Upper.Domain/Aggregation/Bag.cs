using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Domain.Aggregation;

/// <summary>
/// 包装袋
/// </summary>
public class Bag : CommonFields
{
    /// <summary>
    /// 生产过程记录
    /// </summary>
    public List<BagProcessRecord> Records { get; set; } = new();

    /// <summary>
    /// 产线
    /// </summary>
    public string LineNo { get; set; } = string.Empty;
    /// <summary>
    /// 工位
    /// </summary>
    public string StationNo { get; set; } = string.Empty;
    /// <summary>
    /// 袋码
    /// </summary>
    public string BagCode { get; set; } = string.Empty;
    /// <summary>
    /// 工单号
    /// </summary>
    public string OrderCode { get; set; } = string.Empty;
    /// <summary>
    /// 配方
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    /// <summary>
    /// 牌号
    /// </summary>
    public string ProductAlias { get; set; } = string.Empty;
    /// <summary>
    /// 标称重量
    /// </summary>
    public float? ProductWeight { get; set; }

    /// <summary>
    /// 实际重量
    /// </summary>
    public float? ProductActualWeight { get; set; }
    /// <summary>
    /// 重量单位
    /// </summary>
    public string ProductWeightUnit { get; set; }
    /// <summary>
    /// 袋偏
    /// </summary>
    public ushort ProductHeight { get; set; }
    /// <summary>
    /// 袋偏单位
    /// </summary>
    public string ProductHeightUnit { get; set; }
    /// <summary>
    /// 批号
    /// </summary>
    public string BatchCode { get; set; }
    /// <summary>
    /// 批次补位
    /// </summary>
    public ushort SeqDigits { get; set; }
    /// <summary>
    /// 垛型
    /// </summary>
    public ushort LoadShape { get; set; }
    /// <summary>
    /// 垛量
    /// </summary>
    public ushort LoadQuantity { get; set; }
    /// <summary>
    /// 是否打印
    /// </summary>
    public bool IsNeedPrint { get; set; }
    /// <summary>
    /// 序列号
    /// </summary>
    public string? SerialNo { get; set; }
    /// <summary>
    /// 垛位
    /// </summary>
    public ushort? LoadPosition { get; set; }
    /// <summary>
    /// 托盘码
    /// </summary>
    public string? PalletCode { get; set; }
    /// <summary>
    /// 喷墨时间
    /// </summary>
    public DateTime? PrintedAt { get; set; }
    /// <summary>
    /// 码垛时间
    /// </summary>
    public DateTime? PalletizedAt { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public string? ProductionAt { get; set; }

    /// <summary>
    /// 添加过程记录
    /// </summary>
    public void AddRecord(ProcessStep step, string machineId, bool isSuccess, string data = "", string operatorName = "PLC")
    {
        Records.Add(new BagProcessRecord
        {
            BagCode = this.BagCode,
            Step = step,
            OccurredTime = DateTime.Now,
            MachineId = machineId,
            IsSuccess = isSuccess,
            Data = data,
            Operator = operatorName
        });
    }

    /// <summary>
    /// 是否允许上袋
    /// </summary>
    //public bool CanLoad()
    //{
    //    // 逻辑：没有成功的上袋记录即可（或者允许重复上袋但需记录）
    //    // 这里假设一个袋码只能上袋一次
    //    return !Records.Any(r => r.Step == ProcessStep.Loading && r.IsSuccess);
    //}

    /// <summary>
    /// 是否允许套袋
    /// </summary>
    //public bool CanBag()
    //{
    //    // 必须有上袋成功记录，且没有套袋成功记录
    //    return Records.Any(r => r.Step == ProcessStep.Loading && r.IsSuccess)
    //        && !Records.Any(r => r.Step == ProcessStep.Bagging && r.IsSuccess);
    //}

    /// <summary>
    /// 是否允许包装/装料
    /// </summary>
    public bool CanPackaging()
    {
        // 逻辑：没有成功的上袋记录即可（或者允许重复上袋但需记录）
        // 这里假设一个袋码只能上袋一次
        return !Records.Any(r => r.Step == ProcessStep.Packaging && r.IsSuccess);
    }

    /// <summary>
    /// 是否允许复检称重
    /// </summary>
    public bool CanWeigh()
    {
        // 必须有包装成功记录
        // 复检可以多次，所以不检查是否已复检
        return Records.Any(r => r.Step == ProcessStep.Packaging && r.IsSuccess);
    }

    /// <summary>
    /// 是否允许喷码
    /// </summary>
    public bool CanInkjet()
    {
        // 必须有复检称重成功记录（合格）
        // 这里假设 IsSuccess=true 代表称重合格
        return Records.Any(r => r.Step == ProcessStep.Weighing && r.IsSuccess)
            && !Records.Any(r => r.Step == ProcessStep.Inkjet && r.IsSuccess);
    }

    /// <summary>
    /// 是否允许码垛
    /// </summary>
    public bool CanPalletize()
    {
        // 必须有喷码成功记录
        return Records.Any(r => r.Step == ProcessStep.Inkjet && r.IsSuccess)
            && !Records.Any(r => r.Step == ProcessStep.Palletizing && r.IsSuccess);
    }

    public bool CanPalletOut()
    {
        // 必须有码垛成功记录
        return Records.Any(r => r.Step == ProcessStep.Palletizing && r.IsSuccess)
            && !Records.Any(r => r.Step == ProcessStep.PalletOut && r.IsSuccess);
    }

    public bool CanLabeling()
    {
        // 必须有码垛成功记录，且没有贴标成功记录
        return Records.Any(r => r.Step == ProcessStep.Palletizing && r.IsSuccess)
            && !Records.Any(r => r.Step == ProcessStep.Labeling && r.IsSuccess);
    }
}