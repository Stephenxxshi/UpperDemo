using Plant01.Domain.Shared.Models.Equipment;

namespace Plant01.Upper.Domain.Entities;

/// <summary>
/// 产线实体
/// </summary>
public class ProductionLine
{
    public int Id { get; set; }

    /// <summary>
    /// 产线编号（唯一标识，如 Line01）
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// 产线名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// AGV设备编号（对应设备配置中的设备编号）
    /// </summary>
    public string AgvDeviceCode { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 策略配置（JSON格式，原工段策略移至此处）
    /// </summary>
    public string? StrategyConfigJson { get; set; }

    /// <summary>
    /// 包含的工位列表
    /// </summary>
    public List<Workstation> Workstations { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public List<Equipment> GetEquipmentByCapabilities(Capabilities capabilities, string? stationCode = null)
    {
        var equipments = Workstations
            .SelectMany(ws => ws.Equipments)
            .Where(e => e.Capabilities.HasFlag(capabilities));
        if (stationCode != null)
        {
            return equipments.Where(e => e.Code == stationCode).ToList();
        }
        return equipments.ToList();

    }
}
