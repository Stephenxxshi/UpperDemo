using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Plant01.Domain.Shared.Models.Equipment;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Infrastructure.Configs.Models;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 设备配置服务 - 从配置文件加载设备模板和标签映射
/// </summary>
public class EquipmentConfigService : IEquipmentConfigService
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
                var equipmentFile = Path.Combine(_configPath, "Lines", "Equipments", "equipment_templates.csv");
                if (File.Exists(equipmentFile))
                {
                    var lines = File.ReadAllLines(equipmentFile);
                    var equipments = new List<EquipmentTemplateDto>();
                    
                    // Skip header
                    foreach (var line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        var parts = ParseCsvLine(line);
                        if (parts.Count < 6) continue;

                        equipments.Add(new EquipmentTemplateDto
                        {
                            Code = parts[0].Trim(),
                            Name = parts[1].Trim(),
                            Type = parts[2].Trim(),
                            Capabilities = parts[3].Trim().Trim('"'), // Remove quotes if present
                            Sequence = int.TryParse(parts[4], out var seq) ? seq : 0,
                            Enabled = bool.TryParse(parts[5], out var enabled) ? enabled : true
                        });
                    }

                    _equipmentCache = equipments.ToDictionary(e => e.Code, StringComparer.OrdinalIgnoreCase);
                    _logger.LogInformation("已加载 {Count} 个设备模板", _equipmentCache.Count);
                }
                else
                {
                    _logger.LogWarning("设备模板文件不存在: {Path}", equipmentFile);
                }

                // 加载设备映射
                var mappingFile = Path.Combine(_configPath, "Lines", "Equipments", "equipment_mappings.csv");
                if (File.Exists(mappingFile))
                {
                    var lines = File.ReadAllLines(mappingFile);
                    var allMappings = new List<(string EquipmentCode, TagMappingDto Mapping)>();

                    // Skip header
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = ParseCsvLine(line);
                        if (parts.Count < 7) continue;

                        var equipmentCode = parts[6].Trim();
                        if (string.IsNullOrEmpty(equipmentCode))
                        {
                            _logger.LogWarning("CSV行 {LineNumber} 缺少 EquipmentCode: {Line}", i + 1, line);
                            continue;
                        }

                        var mapping = new TagMappingDto
                        {
                            TagName = parts[0].Trim(),
                            Purpose = parts[1].Trim(),
                            IsCritical = bool.TryParse(parts[2], out var isCritical) && isCritical,
                            Direction = ParseTagDirection(parts[3].Trim()),
                            TriggerCondition = parts[4].Trim(),
                            Remarks = parts[5].Trim(),
                            IsTrigger = !string.IsNullOrWhiteSpace(parts[4])
                        };
                        
                        // Fix for IsTrigger logic based on JSON observation
                        if (!string.IsNullOrEmpty(mapping.TriggerCondition))
                        {
                            mapping.IsTrigger = true;
                        }

                        allMappings.Add((equipmentCode, mapping));
                    }

                    _mappingCache = allMappings
                        .GroupBy(x => x.EquipmentCode)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.Mapping).ToList(),
                            StringComparer.OrdinalIgnoreCase);

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

    private List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current);
        return result;
    }

    private TagDirection ParseTagDirection(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return TagDirection.Input;
        if (input.Equals("Output", StringComparison.OrdinalIgnoreCase) || 
            input.Equals("OutPut", StringComparison.OrdinalIgnoreCase)) // Handle typo in CSV
        {
            return TagDirection.Output;
        }
        return TagDirection.Input;
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
