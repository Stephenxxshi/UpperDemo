using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Domain.Models.DeviceCommunication;

using System.Globalization;
using System.Text.Json;

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

                    if (configDict.TryGetValue("Name", out var name)) config.Name = name?.ToString() ?? "";
                    if (configDict.TryGetValue("Drive", out var drive)) config.DriverType = drive?.ToString() ?? "";
                    if (configDict.TryGetValue("Enable", out var enable) && bool.TryParse(enable?.ToString(), out bool e)) config.Enabled = e;

                    // Create a default device for this channel based on old config
                    // Assuming 1-to-1 mapping for now as per old structure
                    var device = new DeviceConfig
                    {
                        Name = config.Name, // Use channel name as device name for now
                        Enabled = config.Enabled
                    };

                    // Map old properties to Options
                    if (configDict.TryGetValue("Address", out var addr)) device.Options["IpAddress"] = addr?.ToString() ?? "";
                    if (configDict.TryGetValue("Port", out var port) && int.TryParse(port?.ToString(), out int p)) device.Options["Port"] = p;
                    if (configDict.TryGetValue("ScanRate", out var rate) && int.TryParse(rate?.ToString(), out int r)) device.Options["ScanRate"] = r;
                    
                    // Also copy other properties that might be driver specific
                    foreach(var kvp in configDict)
                    {
                        if (kvp.Key != "Name" && kvp.Key != "Drive" && kvp.Key != "Enable" && kvp.Key != "Address" && kvp.Key != "Port")
                        {
                            device.Options[kvp.Key] = kvp.Value;
                        }
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
}
