namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 设备-标签映射（将业务设备与通信标签关联）
/// </summary>
public class EquipmentTagMapping
{
    public int Id { get; set; }
    
    /// <summary>
    /// 设备ID
    /// </summary>
    public int EquipmentId { get; set; }
    
    /// <summary>
    /// 设备
    /// </summary>
    public Equipment? Equipment { get; set; }
    
    /// <summary>
    /// 标签名称（通信层标签全名，如 SDJ01.Heartbeat）
    /// </summary>
    public required string TagName { get; set; }
    
    /// <summary>
    /// 标签用途（如 Heartbeat, Alarm, Output, Mode, Recipe 等）
    /// </summary>
    public required string Purpose { get; set; }
    
    /// <summary>
    /// 关联的通道名称（可选，用于快速定位）
    /// </summary>
    public string? ChannelName { get; set; }
    
    /// <summary>
    /// 是否为关键标签（用于监控优先级）
    /// </summary>
    public bool IsCritical { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 标签用途枚举（常见用途预定义）
/// </summary>
public static class TagPurpose
{
    public const string Heartbeat = "Heartbeat";      // 心跳
    public const string Alarm = "Alarm";              // 报警
    public const string AlarmCode = "AlarmCode";      // 报警码
    public const string OutputCount = "OutputCount";  // 产量
    public const string Mode = "Mode";                // 模式
    public const string Status = "Status";            // 状态
    public const string Recipe = "Recipe";            // 配方
    public const string Quality = "Quality";          // 质量
    public const string Power = "Power";              // 电源状态
    public const string Speed = "Speed";              // 速度
    public const string Temperature = "Temperature";  // 温度
    public const string Pressure = "Pressure";        // 压力
}
