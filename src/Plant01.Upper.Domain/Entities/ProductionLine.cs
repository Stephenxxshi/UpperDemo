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
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 包含的工段列表
    /// </summary>
    public List<ProductionSection> Sections { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
