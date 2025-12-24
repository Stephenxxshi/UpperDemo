namespace Plant01.Upper.Infrastructure.Configs.Models;

/// <summary>
/// 产线配置 DTO（用于反序列化 production_lines.json）
/// </summary>
public class ProductionLineDto
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool Enabled { get; set; }
    public string? Description { get; set; }
    public string? StrategyConfigJson { get; set; }
    public List<WorkstationDto> Workstations { get; set; } = new();
}

/// <summary>
/// 工位配置 DTO（使用 EquipmentRefs 引用设备）
/// </summary>
public class WorkstationDto
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Type { get; set; }
    public int Sequence { get; set; }
    public List<string> EquipmentRefs { get; set; } = new();
}
