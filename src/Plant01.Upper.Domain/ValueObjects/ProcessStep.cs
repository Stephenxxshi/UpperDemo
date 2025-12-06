namespace Plant01.Upper.Domain.ValueObjects;

/// <summary>
/// 生产工序步骤
/// </summary>
public enum ProcessStep
{
    /// <summary>
    /// 上袋
    /// </summary>
    Loading = 1,
    
    /// <summary>
    /// 套袋
    /// </summary>
    Bagging = 2,
    
    /// <summary>
    /// 包装/装料
    /// </summary>
    Filling = 3,
    
    /// <summary>
    /// 复检称重
    /// </summary>
    Weighing = 4,
    
    /// <summary>
    /// 喷码
    /// </summary>
    Printing = 5,
    
    /// <summary>
    /// 码垛
    /// </summary>
    Palletizing = 6,
    
    /// <summary>
    /// 出垛
    /// </summary>
    PalletOut = 7
}
