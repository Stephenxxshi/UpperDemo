using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Plant01.Domain.Shared.Models.Equipment;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Models;
using Plant01.Upper.Domain.Entities;

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

    private readonly MultiFormatConfigLoader _configLoader;

    public EquipmentConfigService(
        IConfiguration configuration,
        ILogger<EquipmentConfigService> logger,
        MultiFormatConfigLoader configLoader)
    {
        _configPath = configuration["ConfigsPath"] ??
                      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        _logger = logger;
        _configLoader = configLoader;
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
                // 加载设备模板 (支持 CSV 和 JSON)
                var equipmentDir = Path.Combine(_configPath, "Lines", "Equipments");
                var equipments = _configLoader.LoadFromDirectory<EquipmentTemplateDto>(equipmentDir);

                _equipmentCache = equipments.GroupBy(e => e.Code) // 防止重复
                                            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                _logger.LogInformation("[ 设备配置服务 ] 已加载 {Count} 个设备模板", _equipmentCache.Count);

                // 加载设备映射 (支持 CSV 和 JSON)
                var tagsDir = Path.Combine(_configPath, "Lines", "Tags");
                var rows = _configLoader.LoadFromDirectory<EquipmentMappingCsvRow>(tagsDir);

                var allMappings = new List<(string EquipmentCode, TagMappingDto Mapping)>();

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.EquipmentCode))
                    {
                        _logger.LogWarning("[ 设备配置服务 ] 映射配置行缺少 EquipmentCode: {TagName}", row.TagName);
                        continue;
                    }

                    var mapping = new TagMappingDto
                    {
                        TagCode = row.TagCode?.Trim() ?? "",
                        TagName = row.TagName?.Trim() ?? "",
                        Purpose = row.Purpose?.Trim() ?? "",
                        DataType = ParseFinalDataType(row.DataType),
                        ValueTransformExpression = row.ValueTransformExpression?.Trim(),
                        IsCritical = row.IsCritical,
                        Direction = ParseTagDirection(row.Direction),
                        TriggerCondition = row.TriggerCondition?.Trim(),
                        Remarks = row.Remarks?.Trim(),
                        IsTrigger = !string.IsNullOrWhiteSpace(row.TriggerCondition)
                    };

                    allMappings.Add((row.EquipmentCode.Trim(), mapping));
                }

                _mappingCache = allMappings
                    .GroupBy(x => x.EquipmentCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.Mapping).ToList(),
                        StringComparer.OrdinalIgnoreCase);

                _logger.LogInformation("[ 设备配置服务 ] 已加载 {Count} 个设备映射配置", _mappingCache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ 设备配置服务 ] 加载设备配置文件失败");
            }
        }
    }

    public class EquipmentMappingCsvRow
    {
        public string TagCode { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        [CsvHelper.Configuration.Attributes.Optional]
        public string DataType { get; set; } = string.Empty;
        [CsvHelper.Configuration.Attributes.Optional]
        public string ValueTransformExpression { get; set; } = string.Empty;
        [CsvHelper.Configuration.Attributes.Optional]
        [CsvHelper.Configuration.Attributes.Default(false)]
        public bool IsCritical { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string TriggerCondition { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string EquipmentCode { get; set; } = string.Empty;
    }

    private FinalDataType ParseFinalDataType(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return FinalDataType.Int16;

        if (Enum.TryParse<FinalDataType>(input, true, out var dataType))
        {
            return dataType;
        }

        if (input.Equals("Short", StringComparison.OrdinalIgnoreCase)) return FinalDataType.Int16;
        if (input.Equals("UShort", StringComparison.OrdinalIgnoreCase)) return FinalDataType.UInt16;
        if (input.Equals("Int", StringComparison.OrdinalIgnoreCase)) return FinalDataType.Int32;
        if (input.Equals("UInt", StringComparison.OrdinalIgnoreCase)) return FinalDataType.UInt32;
        if (input.Equals("Bool", StringComparison.OrdinalIgnoreCase)) return FinalDataType.Boolean;
        if (input.Equals("Real", StringComparison.OrdinalIgnoreCase)) return FinalDataType.Float;

        _logger.LogWarning("[ 设备配置服务 ] 未知标签数据类型: {DataType}, 使用默认 Int16", input);
        return FinalDataType.Int16;
    }

    private TagDirection ParseTagDirection(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return TagDirection.Input;
        if (input.Equals("Output", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("OutPut", StringComparison.OrdinalIgnoreCase)) // 处理CSV中的拼写错误
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
            _logger.LogWarning("[ 设备配置服务 ] 未找到设备模板: {Code}", code);
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
                TagCode = m.TagCode,
                Purpose = m.Purpose,
                DataType = m.DataType,
                ValueTransformExpression = m.ValueTransformExpression,
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
        _logger.LogInformation("[ 设备配置服务 ] 正在重新加载设备配置...");
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
        _logger.LogWarning("[ 设备配置服务 ] 未知的设备类型: {Type}, 使用默认值 Unknown", typeStr);
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
                _logger.LogWarning("[ 设备通信服务 ] 未知的设备能力: {Capability}", cap);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取指定工位的所有设备配置 (通过 StationCode 反向查找)
    /// </summary>
    public List<EquipmentTemplateDto> GetEquipmentsByStationCode(string stationCode)
    {
        return _equipmentCache.Values
            .Where(e => string.Equals(e.StationCode, stationCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    List<string> IEquipmentConfigService.GetAllEquipmentCodes()
    {
        return _equipmentCache.Keys.ToList();
    }

    //public List<Equipment> GetEquipmentByPurpose(string purpose)
    //{
    //    return _equipmentCache.Values
    //        .Where(e => e.TagMappings.Any(m => m.Purpose == purpose))
    //        .ToList();
    //}
}
