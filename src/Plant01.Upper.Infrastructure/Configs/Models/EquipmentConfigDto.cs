using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Infrastructure.Configs.Models;

/// <summary>
/// 设备模板配置 DTO
/// </summary>
public class EquipmentTemplateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Capabilities { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public bool Enabled { get; set; } = true;
    public string? ConfigJson { get; set; }
}

/// <summary>
/// 设备到标签的映射配置 DTO
/// </summary>
public class EquipmentMappingDto
{
    public string EquipmentCode { get; set; } = string.Empty;
    public List<TagMappingDto> TagMappings { get; set; } = new();
}

/// <summary>
/// 标签映射配置 DTO
/// </summary>
public class TagMappingDto
{
    public string TagName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? ChannelName { get; set; }
    public bool IsCritical { get; set; }
    public TagDirection Direction { get; set; } = TagDirection.Input;
    public bool IsTrigger { get; set; }
    public string? TriggerCondition { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// 工位引用配置 DTO（用于简化的 production_lines.json）
/// </summary>
public class WorkstationRefDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public List<string> EquipmentRefs { get; set; } = new();
}
