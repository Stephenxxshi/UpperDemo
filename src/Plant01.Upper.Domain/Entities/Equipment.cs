using Plant01.Domain.Shared.Models.Equipment;

namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 设备实体（业务层设备概念）
/// </summary>
public class Equipment
{
    public int Id { get; set; }
    
    /// <summary>
    /// 设备编号（唯一标识，如 SDJ01, BZJ01）
    /// </summary>
    public required string Code { get; set; }
    
    /// <summary>
    /// 设备名称
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// 设备类型
    /// </summary>
    public EquipmentType Type { get; set; }
    
    /// <summary>
    /// 设备能力（组合）
    /// </summary>
    public Capabilities Capabilities { get; set; }
    
    /// <summary>
    /// 所属工站ID
    /// </summary>
    public int WorkstationId { get; set; }
    
    /// <summary>
    /// 所属工站
    /// </summary>
    public Workstation? Workstation { get; set; }
    
    /// <summary>
    /// 设备状态
    /// </summary>
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Offline;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// 排序序号
    /// </summary>
    public int Sequence { get; set; }
    
    /// <summary>
    /// 设备配置参数（JSON）
    /// </summary>
    public string? ConfigJson { get; set; }
    
    /// <summary>
    /// 该设备关联的标签映射列表
    /// </summary>
    public List<EquipmentTagMapping> TagMappings { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

public enum EquipmentStatus
{
    Offline = 0,    // 离线
    Online = 1,     // 在线
    Running = 2,    // 运行中
    Alarm = 3,      // 报警
    Fault = 4,      // 故障
    Maintenance = 5 // 维护中
}
