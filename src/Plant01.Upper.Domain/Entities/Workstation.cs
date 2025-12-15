using Plant01.Domain.Shared.Models.Equipment;

namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 工位/工站实体
/// </summary>
public class Workstation
{
    public int Id { get; set; }
    
    /// <summary>
    /// 工站编号（唯一标识）
    /// </summary>
    public required string Code { get; set; }
    
    /// <summary>
    /// 工站名称
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// 所属工段/区域编号
    /// </summary>
    public string? SectionCode { get; set; }
    
    /// <summary>
    /// 工站状态
    /// </summary>
    public WorkstationStatus Status { get; set; } = WorkstationStatus.Idle;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// 排序序号
    /// </summary>
    public int Sequence { get; set; }
    
    /// <summary>
    /// 该工站包含的设备列表
    /// </summary>
    public List<Equipment> Equipments { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

public enum WorkstationStatus
{
    Idle = 0,       // 空闲
    Running = 1,    // 运行中
    Alarm = 2,      // 报警
    Maintenance = 3 // 维护中
}
