using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Domain.Aggregation;

/// <summary>
/// 托盘聚合根
/// </summary>
public class Pallet : CommonFields
{
    /// <summary>
    /// 托盘码 (唯一标识)
    /// </summary>
    public string PalletCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 关联工单号
    /// </summary>
    public string WorkOrderCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否满垛
    /// </summary>
    public bool IsFull { get; set; }
    
    /// <summary>
    /// 出垛时间
    /// </summary>
    public DateTime? OutTime { get; set; }
    
    /// <summary>
    /// 当前所在的码垛机编号
    /// </summary>
    public string CurrentPalletizerId { get; set; } = string.Empty;
    
    /// <summary>
    /// 托盘内的包装袋列表
    /// </summary>
    public List<PalletItem> Items { get; set; } = new();
    
    /// <summary>
    /// 添加包装袋到托盘
    /// </summary>
    public void AddBag(string bagCode, int positionIndex)
    {
        if (Items.Any(x => x.BagCode == bagCode)) return;
        
        Items.Add(new PalletItem
        {
            PalletCode = this.PalletCode,
            BagCode = bagCode,
            PositionIndex = positionIndex,
            AddedTime = DateTime.Now
        });
    }
}
