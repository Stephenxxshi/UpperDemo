namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 托盘内的单包明细
/// </summary>
public class PalletItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// 关联的托盘码
    /// </summary>
    public string PalletCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 关联的袋码
    /// </summary>
    public string BagCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 在托盘中的位置索引 (1-N)
    /// </summary>
    public int PositionIndex { get; set; }
    
    /// <summary>
    /// 放入时间
    /// </summary>
    public DateTime AddedTime { get; set; }
}
