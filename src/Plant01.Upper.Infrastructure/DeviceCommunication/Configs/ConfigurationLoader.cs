using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Domain.Models.DeviceCommunication;

using System.Globalization;
using System.Text.Json;
using Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Configs;

public class ConfigurationLoader
{
    private readonly string _configsPath;
    private readonly ILogger<ConfigurationLoader> _logger;

    public ConfigurationLoader(string configsPath, ILogger<ConfigurationLoader> logger)
    {
        _configsPath = configsPath;
        _logger = logger;
    }

    public List<ChannelConfig> LoadChannels()
    {
        var channels = new List<ChannelConfig>();
        var channelsPath = Path.Combine(_configsPath, "Channels");

        if (!Directory.Exists(channelsPath))
        {
            _logger.LogWarning("未找到通道目录: {Path}", channelsPath);
            return channels;
        }

        var files = Directory.GetFiles(channelsPath, "*.json");
        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);

                if (configDict != null)
                {
                    var config = new ChannelConfig();

                    // 提取 Channel 级别的属性
                    if (configDict.TryGetValue("Name", out var name)) config.Name = name?.ToString() ?? "";
                    if (configDict.TryGetValue("Drive", out var drive)) config.DriverType = drive?.ToString() ?? "";
                    if (configDict.TryGetValue("Enable", out var enable) && bool.TryParse(enable?.ToString(), out bool e)) config.Enabled = e;

                    // 为此通道创建默认设备（根据旧结构假设 1:1 映射）
                    var device = new DeviceConfig
                    {
                        Name = config.Name, // 暂时使用通道名称作为设备名称
                        Enabled = config.Enabled
                    };

                    // 将所有其他配置项原封不动地放入 Options 字典
                    // 不做任何字段映射或类型转换，完全由驱动自己解释
                    foreach (var kvp in configDict)
                    {
                        // 跳过 Channel 级别的属性
                        if (kvp.Key == "Name" || kvp.Key == "Drive" || kvp.Key == "Enable")
                            continue;

                        // 所有驱动特定的配置都放进 Options
                        device.Options[kvp.Key] = kvp.Value;
                    }

                    config.Devices.Add(device);
                    channels.Add(config);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载通道配置失败: {File}", file);
            }
        }

        return channels;
    }

    public List<Tag> LoadTags()
    {
        var tags = new List<Tag>();
        var tagsFile = Path.Combine(_configsPath, "tags.csv");

        if (!File.Exists(tagsFile))
        {
            _logger.LogWarning("未找到标签文件: {Path}", tagsFile);
            return tags;
        }

        try
        {
            using var reader = new StreamReader(tagsFile);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                MissingFieldFound = null,
                HeaderValidated = null
            });

            csv.Context.RegisterClassMap<TagMap>();
            tags = csv.GetRecords<Tag>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从 {File} 加载标签失败", tagsFile);
        }

        return tags;
    }

    // 生成规则模式的标签预览（不落盘）
    public List<ScannedTag> GenerateTagsPreviewByRules(AddressRules rules)
    {
        var scanner = new S7AddressScanner();
        var task = scanner.GenerateByRulesAsync(rules);
        task.Wait();
        return task.Result;
    }

    // 合并扫描结果到 tags.csv，自动备份原文件
    public bool MergeTagsToCsv(IEnumerable<ScannedTag> scanned, string? backupSuffix = null)
    {
        var tagsFile = Path.Combine(_configsPath, "tags.csv");
        Directory.CreateDirectory(Path.GetDirectoryName(tagsFile)!);

        // 备份
        if (File.Exists(tagsFile))
        {
            var backup = tagsFile + "." + (backupSuffix ?? DateTime.Now.ToString("yyyyMMddHHmmss")) + ".bak";
            File.Copy(tagsFile, backup, true);
            _logger.LogInformation("已备份标签文件到: {Backup}", backup);
        }

        var existing = LoadTags();
        var dict = existing.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var s in scanned)
        {
            var dataType = ParseDataType(s.DataType);
            var arrayLen = (ushort)Math.Max(1, s.Length);
            if (string.Equals(s.DataType, "String", StringComparison.OrdinalIgnoreCase))
            {
                // 对于字符串，将 Length 作为字符串长度存入 ArrayLength（按当前 CSV 映射约定）
                arrayLen = (ushort)Math.Max(1, s.Length);
            }

            if (dict.TryGetValue(s.TagName, out var t))
            {
                t.Address = s.Address;
                t.DataType = dataType;
                t.ArrayLength = arrayLen;
                if (!string.IsNullOrWhiteSpace(s.RW))
                {
                    t.AccessRights = s.RW.Equals("R", StringComparison.OrdinalIgnoreCase) ? AccessRights.Read
                        : s.RW.Equals("W", StringComparison.OrdinalIgnoreCase) ? AccessRights.Write
                        : AccessRights.ReadWrite;
                }
            }
            else
            {
                existing.Add(new Tag
                {
                    Name = s.TagName,
                    Address = s.Address,
                    DataType = dataType,
                    ArrayLength = arrayLen,
                    AccessRights = string.IsNullOrWhiteSpace(s.RW) ? AccessRights.Read :
                        s.RW.Equals("R", StringComparison.OrdinalIgnoreCase) ? AccessRights.Read :
                        s.RW.Equals("W", StringComparison.OrdinalIgnoreCase) ? AccessRights.Write : AccessRights.ReadWrite
                });
            }
        }

        try
        {
            using var writer = new StreamWriter(tagsFile);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = args => true
            });
            csv.Context.RegisterClassMap<TagMap>();
            csv.WriteRecords(existing);
            _logger.LogInformation("已合并并写入标签文件: {File}", tagsFile);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入标签文件失败: {File}", tagsFile);
            return false;
        }
    }

    // 读取 DB 结构 schema 并生成预览（优先使用该模式）
    public List<ScannedTag> GenerateTagsPreviewFromDbSchema(int dbNumber)
    {
        var schemaPath = Path.Combine(_configsPath, "DbSchemas", $"DB{dbNumber}.schema.json");
        if (!File.Exists(schemaPath))
        {
            _logger.LogWarning("未找到 DB 结构定义: {Path}", schemaPath);
            return new List<ScannedTag>();
        }

        try
        {
            var json = File.ReadAllText(schemaPath);
            var schema = JsonSerializer.Deserialize<S7DbSchema>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (schema == null)
            {
                _logger.LogWarning("DB 结构定义解析为空: {Path}", schemaPath);
                return new List<ScannedTag>();
            }

            var scanner = new S7AddressScanner();
            var task = scanner.GenerateFromSchemaAsync(schema);
            task.Wait();
            return task.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析 DB 结构定义失败: {Path}", schemaPath);
            return new List<ScannedTag>();
        }
    }

    private static TagDataType ParseDataType(string typeStr)
    {
        if (Enum.TryParse<TagDataType>(typeStr, true, out var result))
            return result;
        if (string.Equals(typeStr, "Short", StringComparison.OrdinalIgnoreCase)) return TagDataType.Int16;
        if (string.Equals(typeStr, "UShort", StringComparison.OrdinalIgnoreCase)) return TagDataType.UInt16;
        if (string.Equals(typeStr, "Int", StringComparison.OrdinalIgnoreCase)) return TagDataType.Int32;
        if (string.Equals(typeStr, "UInt", StringComparison.OrdinalIgnoreCase)) return TagDataType.UInt32;
        if (string.Equals(typeStr, "Bool", StringComparison.OrdinalIgnoreCase)) return TagDataType.Boolean;
        if (string.Equals(typeStr, "Real", StringComparison.OrdinalIgnoreCase)) return TagDataType.Float;
        if (string.Equals(typeStr, "DInt", StringComparison.OrdinalIgnoreCase)) return TagDataType.Int32;
        if (string.Equals(typeStr, "Word", StringComparison.OrdinalIgnoreCase)) return TagDataType.UInt16;
        if (string.Equals(typeStr, "DWord", StringComparison.OrdinalIgnoreCase)) return TagDataType.UInt32;
        return TagDataType.Int16;
    }
}
