using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plant01.Domain.Shared.Models.Equipment;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Infrastructure.Configs.Models;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 设备配置服务 - 从配置文件加载设备模板和标签映射
/// </summary>
public class EquipmentConfigService
{
    private readonly string _configPath;
    private readonly ILogger<EquipmentConfigService> _logger;
    
    // 内存缓存
    private Dictionary<string, EquipmentTemplateDto> _equipmentCache = new();
    private Dictionary<string, List<TagMappingDto>> _mappingCache = new();
    private readonly object _lock = new();

    public EquipmentConfigService(
        IConfiguration configuration,
        ILogger<EquipmentConfigService> logger)
    {
        _configPath = configuration["ConfigsPath"] ?? 
                      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        _logger = logger;
        LoadConfigs();
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    private void LoadConfigs()
    {
        lock (_lock)
        {
            try
            {
                // 加载设备模板
                var equipmentFile = Path.Combine(_configPath, "Equipments", "equipment_templates.json");
                if (File.Exists(equipmentFile))
                {
                    var json = File.ReadAllText(equipmentFile);
                    var equipments = JsonSerializer.Deserialize<List<EquipmentTemplateDto>>(json, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _equipmentCache = equipments?.ToDictionary(e => e.Code, StringComparer.OrdinalIgnoreCase) 
                        ?? new();
                    _logger.LogInformation("已加载 {Count} 个设备模板", _equipmentCache.Count);
                }
                else
                {
                    _logger.LogWarning("设备模板文件不存在: {Path}", equipmentFile);
                }

                // 加载设备映射
                var mappingFile = Path.Combine(_configPath, "Equipments", "equipment_mappings.json");
                if (File.Exists(mappingFile))
                {
                    var json = File.ReadAllText(mappingFile);
                    var mappings = JsonSerializer.Deserialize<List<EquipmentMappingDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _mappingCache = mappings?.ToDictionary(
                        m => m.EquipmentCode,
                        m => m.TagMappings,
                        StringComparer.OrdinalIgnoreCase) ?? new();
                    _logger.LogInformation("已加载 {Count} 个设备映射配置", _mappingCache.Count);
                }
                else
                {
                    _logger.LogWarning("设备映射文件不存在: {Path}", mappingFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载设备配置文件失败");
            }
        }
    }

    /// <summary>
    /// 根据设备编码获取设备实例（包含标签映射）
    /// </summary>
    public Equipment? GetEquipment(string code)
    {
        if (!_equipmentCache.TryGetValue(code, out var template))
        {
            _logger.LogWarning("未找到设备模板: {Code}", code);
            return null;
        }

        var equipment = new Equipment
        {
            Code = template.Code,
            Name = template.Name,
            Type = ParseEquipmentType(template.Type),
            Capabilities = ParseCapabilities(template.Capabilities),
            Sequence = template.Sequence,
            Enabled = template.Enabled,
            ConfigJson = template.ConfigJson
        };

        // 加载标签映射（运行时，不持久化到数据库）
        if (_mappingCache.TryGetValue(code, out var mappings))
        {
            equipment.TagMappings = mappings.Select(m => new EquipmentTagMapping
            {
                TagName = m.TagName,
                Purpose = m.Purpose,
                ChannelName = m.ChannelName,
                IsCritical = m.IsCritical,
                Direction = m.Direction,
                IsTrigger = m.IsTrigger,
                TriggerCondition = m.TriggerCondition,
                Remarks = m.Remarks
            }).ToList();
        }

        return equipment;
    }

    /// <summary>
    /// 批量获取设备
    /// </summary>
    public List<Equipment> GetEquipmentsByRefs(List<string> refs)
    {
        return refs.Select(GetEquipment)
                   .Where(e => e != null)
                   .Cast<Equipment>()
                   .ToList();
    }

    /// <summary>
    /// 获取设备的标签映射
    /// </summary>
    public List<TagMappingDto> GetMappings(string equipmentCode)
    {
        return _mappingCache.GetValueOrDefault(equipmentCode, new List<TagMappingDto>());
    }

    /// <summary>
    /// 获取所有设备编码
    /// </summary>
    public IEnumerable<string> GetAllEquipmentCodes()
    {
        return _equipmentCache.Keys;
    }

    /// <summary>
    /// 获取所有设备映射配置（用于触发标签扫描）
    /// </summary>
    public List<EquipmentMappingDto> GetAllMappings()
    {
        return _mappingCache.Select(kvp => new EquipmentMappingDto
        {
            EquipmentCode = kvp.Key,
            TagMappings = kvp.Value
        }).ToList();
    }

    /// <summary>
    /// 重新加载配置
    /// </summary>
    public void Reload()
    {
        _logger.LogInformation("正在重新加载设备配置...");
        LoadConfigs();
    }

    /// <summary>
    /// 解析设备类型
    /// </summary>
    private EquipmentType ParseEquipmentType(string typeStr)
    {
        if (Enum.TryParse<EquipmentType>(typeStr, true, out var type))
        {
            return type;
        }
        _logger.LogWarning("未知的设备类型: {Type}, 使用默认值 Unknown", typeStr);
        return EquipmentType.Unknown;
    }

    /// <summary>
    /// 解析设备能力（支持组合）
    /// </summary>
    private Capabilities ParseCapabilities(string capabilitiesStr)
    {
        if (string.IsNullOrWhiteSpace(capabilitiesStr))
            return Capabilities.None;

        var result = Capabilities.None;
        foreach (var cap in capabilitiesStr.Split(',', StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<Capabilities>(cap, true, out var capability))
            {
                result |= capability;
            }
            else
            {
                _logger.LogWarning("未知的设备能力: {Capability}", cap);
            }
        }
        return result;
    }
}
