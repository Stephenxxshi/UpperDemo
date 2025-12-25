using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.Extensions.Logging;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;
using Plant01.Upper.Infrastructure.DeviceCommunication.Parsers;
using Plant01.Upper.Infrastructure.Services; // Add this

using System.Globalization;
using System.Text.Json;
using Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;
using System.Text;
using Plant01.Upper.Application.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Configs;

public class ConfigurationLoader
{
    private readonly string _configsPath;
    private readonly ILogger<ConfigurationLoader> _logger;
    private readonly Dictionary<string, IDriverTagParser> _tagParsers;
    private readonly MultiFormatConfigLoader _multiFormatLoader; // Add this

    public ConfigurationLoader(string configsPath, ILogger<ConfigurationLoader> logger, MultiFormatConfigLoader multiFormatLoader) // Inject MultiFormatConfigLoader
    {
        _configsPath = configsPath;
        _logger = logger;
        _multiFormatLoader = multiFormatLoader; // Initialize
        
        // 注册驱动标签解析器
        _tagParsers = new Dictionary<string, IDriverTagParser>(StringComparer.OrdinalIgnoreCase);
        RegisterParser(new S7TagParser());
        // RegisterParser(new ModbusTagParser()); // 将来添加
    }

    private void RegisterParser(IDriverTagParser parser)
    {
        _tagParsers[parser.DriverType] = parser;
    }

    public List<ChannelConfig> LoadChannels()
    {
        var channels = new List<ChannelConfig>();
        var channelsPath = Path.Combine(_configsPath, "DeviceCommunications", "Channels");

        if (!Directory.Exists(channelsPath))
        {
            _logger.LogWarning("[ 加载配置服务 ] 未找到通道目录: {Path}", channelsPath);
            return channels;
        }

        var files = Directory.GetFiles(channelsPath, "*.json");
        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                
                // 使用 Utf8JsonReader 以支持尾随逗号和注释的解析
                var utf8 = Encoding.UTF8.GetBytes(json);
                var readerOptions = new JsonReaderOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                var reader = new Utf8JsonReader(utf8, readerOptions);

                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                var config = new ChannelConfig();
                
                // 提取 Channel 级别的标准字段
                if (root.TryGetProperty("Code", out var code)) 
                    config.Code = code.GetString() ?? "";
                if (root.TryGetProperty("Name", out var name)) 
                    config.Name = name.GetString() ?? "";
                if (root.TryGetProperty("Enable", out var enable)) 
                    config.Enabled = enable.GetBoolean();
                if (root.TryGetProperty("Description", out var desc)) 
                    config.Description = desc.GetString() ?? "";
                if (root.TryGetProperty("Drive", out var drive)) 
                    config.DriverType = drive.GetString() ?? "";
                if (root.TryGetProperty("DriveModel", out var driveModel)) 
                    config.DriveModel = driveModel.GetString() ?? "";

                // 解析 Devices 数组
                if (root.TryGetProperty("Devices", out var devicesElement) && 
                    devicesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var deviceElement in devicesElement.EnumerateArray())
                    {
                        var device = ParseDevice(deviceElement);
                        if (device != null)
                        {
                            config.Devices.Add(device);
                        }
                    }
                }

                // 其他 Channel 级别字段放入 Options (如果需要)
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name == "Code" || prop.Name == "Name" || prop.Name == "Enable" || 
                        prop.Name == "Description" || prop.Name == "Drive" || prop.Name == "DriveModel" || 
                        prop.Name == "Devices" || prop.Name == "$schema")
                        continue;
                        
                    config.Options[prop.Name] = JsonElementToObject(prop.Value);
                }

                channels.Add(config);
                _logger.LogInformation("[ 加载配置服务 ] 已加载通道 {Channel},包含 {DeviceCount} 个设备", 
                    config.Name, config.Devices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ 加载配置服务 ] 加载通道配置失败: {File}", file);
            }
        }

        return channels;
    }

    /// <summary>
    /// 解析单个 Device
    /// </summary>
    private DeviceConfig? ParseDevice(JsonElement deviceElement)
    {
        try
        {
            var device = new DeviceConfig();
            
            // 提取 Device 标准字段
            if (deviceElement.TryGetProperty("Name", out var name))
                device.Name = name.GetString() ?? "";
            if (deviceElement.TryGetProperty("Description", out var desc))
                device.Description = desc.GetString() ?? "";
            if (deviceElement.TryGetProperty("Enable", out var enable))
                device.Enabled = enable.GetBoolean();

            // 将所有其他字段放入 Device.Options
            foreach (var prop in deviceElement.EnumerateObject())
            {
                if (prop.Name == "Name" || prop.Name == "Description" || prop.Name == "Enable")
                    continue;
                    
                device.Options[prop.Name] = JsonElementToObject(prop.Value);
            }

            return device;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[ 加载配置服务 ] 解析设备配置失败");
            return null;
        }
    }

    /// <summary>
    /// JsonElement 转换为 object
    /// </summary>
    private static object? JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int i) ? i : 
                                   element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray()
                .Select(JsonElementToObject).ToArray(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),
            _ => null
        };
    }

    public List<CommunicationTag> LoadTags()
    {
        var tags = new List<CommunicationTag>();
        var tagsDir = Path.Combine(_configsPath, "DeviceCommunications", "Tags");

        if (!Directory.Exists(tagsDir))
        {
            _logger.LogWarning("[ 加载配置服务 ] 未找到标签目录: {Path}", tagsDir);
            return tags;
        }

        var files = Directory.GetFiles(tagsDir, "*.*")
            .Where(f => f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            try
            {
                if (file.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    tags.AddRange(LoadTagsFromCsv(file));
                }
                else if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    // Assuming JSON structure matches CommunicationTag properties
                    var jsonTags = _multiFormatLoader.LoadFromFile<CommunicationTag>(file);
                    tags.AddRange(jsonTags);
                    _logger.LogInformation("[ 加载配置服务 ] 从 JSON 加载了 {Count} 个标签: {File}", jsonTags.Count, file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ 加载配置服务 ] 加载标签文件失败: {File}", file);
            }
        }

        return tags;
    }

    private List<CommunicationTag> LoadTagsFromCsv(string filePath)
    {
        var tags = new List<CommunicationTag>();
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                MissingFieldFound = null,
                HeaderValidated = null
            });

            csv.Context.RegisterClassMap<TagMap>();
            
            while (csv.Read())
            {
                var tag = csv.GetRecord<CommunicationTag>();
                if (tag == null) continue;

                foreach (var parser in _tagParsers.Values)
                {
                    var props = parser.ParseExtendedProperties(csv);
                    if (props.Count > 0)
                    {
                        tag.ExtendedProperties ??= new Dictionary<string, object>();
                        foreach (var kvp in props)
                        {
                            tag.ExtendedProperties[kvp.Key] = kvp.Value;
                        }
                    }
                }
                
                tags.Add(tag);
            }
            _logger.LogInformation("[ 加载配置服务 ] 从 CSV 加载了 {Count} 个标签: {File}", tags.Count, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ 加载配置服务 ] 解析 CSV 标签文件失败: {File}", filePath);
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
            _logger.LogInformation("[ 加载配置服务 ] 已备份标签文件到: {Backup}", backup);
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
                    t.AccessRights = s.RW.Equals("R", StringComparison.OrdinalIgnoreCase) ? Models.AccessRights.Read
                        : s.RW.Equals("W", StringComparison.OrdinalIgnoreCase) ? Models.AccessRights.Write
                        : Models.AccessRights.ReadWrite;
                }
            }
            else
            {
                existing.Add(new CommunicationTag
                {
                    Name = s.TagName,
                    Address = s.Address,
                    DataType = dataType,
                    ArrayLength = arrayLen,
                    AccessRights = string.IsNullOrWhiteSpace(s.RW) ? Models.AccessRights.Read :
                        s.RW.Equals("R", StringComparison.OrdinalIgnoreCase) ? Models.AccessRights.Read :
                        s.RW.Equals("W", StringComparison.OrdinalIgnoreCase) ? Models.AccessRights.Write : Models.AccessRights.ReadWrite
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
            _logger.LogInformation("[ 加载配置服务 ] 已合并并写入标签文件: {File}", tagsFile);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ 加载配置服务 ] 写入标签文件失败: {File}", tagsFile);
            return false;
        }
    }

    // 读取 DB 结构 schema 并生成预览（优先使用该模式）
    public List<ScannedTag> GenerateTagsPreviewFromDbSchema(int dbNumber)
    {
        var schemaPath = Path.Combine(_configsPath, "DbSchemas", $"DB{dbNumber}.schema.json");
        if (!File.Exists(schemaPath))
        {
            _logger.LogWarning("[ 加载配置服务 ] 未找到 DB 结构定义: {Path}", schemaPath);
            return new List<ScannedTag>();
        }

        try
        {
            var json = File.ReadAllText(schemaPath);
            var schema = JsonSerializer.Deserialize<S7DbSchema>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (schema == null)
            {
                _logger.LogWarning("[ 加载配置服务 ] DB 结构定义解析为空: {Path}", schemaPath);
                return new List<ScannedTag>();
            }

            var scanner = new S7AddressScanner();
            var task = scanner.GenerateFromSchemaAsync(schema);
            task.Wait();
            return task.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ 加载配置服务 ] 解析 DB 结构定义失败: {Path}", schemaPath);
            return new List<ScannedTag>();
        }
    }

    private static Models.TagDataType ParseDataType(string typeStr)
    {
        if (Enum.TryParse<Models.TagDataType>(typeStr, true, out var result))
            return result;
        if (string.Equals(typeStr, "Short", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.Int16;
        if (string.Equals(typeStr, "UShort", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.UInt16;
        if (string.Equals(typeStr, "Int", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.Int32;
        if (string.Equals(typeStr, "UInt", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.UInt32;
        if (string.Equals(typeStr, "Bool", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.Boolean;
        if (string.Equals(typeStr, "Real", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.Float;
        if (string.Equals(typeStr, "DInt", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.Int32;
        if (string.Equals(typeStr, "Word", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.UInt16;
        if (string.Equals(typeStr, "DWord", StringComparison.OrdinalIgnoreCase)) return Models.TagDataType.UInt32;
        return Models.TagDataType.Int16;
    }
}
