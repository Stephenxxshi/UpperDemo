namespace Plant01.Upper.Domain.ValueObjects;

/// <summary>
/// 生产工序步骤
/// </summary>
public enum ProcessStep
{
    
    /// <summary>
    /// 包装
    /// </summary>
    Packaging,
    
    /// <summary>
    /// 复检称重
    /// </summary>
    Weighing,
    
    /// <summary>
    /// 喷码
    /// </summary>
    Inkjet,
    
    /// <summary>
    /// 码垛
    /// </summary>
    Palletizing,
    
    /// <summary>
    /// 出垛
    /// </summary>
    PalletOut,

    /// <summary>
    /// 贴标
    /// </summary>
    Labeling
}
