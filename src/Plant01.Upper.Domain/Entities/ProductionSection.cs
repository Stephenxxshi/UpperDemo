using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 工段实体（如：包装段、码垛段）
/// </summary>
public class ProductionSection
{
    public int Id { get; set; }

    /// <summary>
    /// 工段编号（如 SEC_PACKING、SEC_PALLETIZING）
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// 工段名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 顺序号（用于排序工段流程，如 1=包装, 2=称重, 3=码垛）
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// 策略配置（JSON格式，用于存储分配策略等）
    /// 例如：{ "AllocationMode": "Dynamic", "Groups": [...] }
    /// </summary>
    public string? StrategyConfigJson { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 所属产线ID
    /// </summary>
    public int ProductionLineId { get; set; }

    /// <summary>
    /// 所属产线
    /// </summary>
    public ProductionLine? ProductionLine { get; set; }

    /// <summary>
    /// 工段下的工位列表
    /// </summary>
    public List<Workstation> Workstations { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
