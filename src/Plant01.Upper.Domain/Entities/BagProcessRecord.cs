using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 包装袋生产过程记录
/// </summary>
public class BagProcessRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// 关联的袋码
    /// </summary>
    public string BagCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 工序步骤
    /// </summary>
    public ProcessStep Step { get; set; }
    
    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTime OccurredTime { get; set; }
    
    /// <summary>
    /// 设备编号 (如: LoadingMachine_01)
    /// </summary>
    public string MachineId { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// 过程数据 (JSON格式或关键值，如称重重量、喷码内容)
    /// </summary>
    public string Data { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作人或PLC标识
    /// </summary>
    public string Operator { get; set; } = string.Empty;
}
