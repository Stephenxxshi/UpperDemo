namespace Plant01.Upper.Domain.ValueObjects;

/// <summary>
/// 公用字段
/// </summary>
public class CommonFields
{
    /// <summary>
    /// 主键
    /// </summary>        
    public int Id { get; set; }

    /// <summary>
    /// 更新于
    /// </summary>        
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 创建于
    /// </summary>        
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 软删除标志，0未删除，1已删除
    /// </summary>
    public int IsDeleted { get; set; }

    public int Version { get; set; }
}
