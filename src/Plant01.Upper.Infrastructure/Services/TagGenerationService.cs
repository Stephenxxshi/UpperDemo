using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Services;
using Plant01.Upper.Infrastructure.DeviceCommunication.Configs;
using Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 标签生成服务实现（基础设施层）
/// </summary>
public class TagGenerationService : ITagGenerationService
{
    private readonly ConfigurationLoader _configLoader;
    private readonly ILogger<TagGenerationService> _logger;

    public TagGenerationService(
        ConfigurationLoader configLoader,
        ILogger<TagGenerationService> logger)
    {
        _configLoader = configLoader;
        _logger = logger;
    }

    public List<object> PreviewFromDbSchema(int dbNumber)
    {
        _logger.LogInformation("正在从 DB{DbNumber} Schema 生成标签预览...", dbNumber);
        try
        {
            var tags = _configLoader.GenerateTagsPreviewFromDbSchema(dbNumber);
            _logger.LogInformation("成功生成 {Count} 个标签预览", tags.Count);
            return tags.Cast<object>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从 DB{DbNumber} Schema 生成标签失败", dbNumber);
            throw;
        }
    }

    public List<object> PreviewFromRules(object rules)
    {
        if (rules is not AddressRules addressRules) throw new ArgumentException("Invalid rules type");

        _logger.LogInformation("正在按规则生成标签预览: DB{DbNumber}, 模板={NameTemplate}",
            addressRules.DbNumber, addressRules.NameTemplate);
        try
        {
            var tags = _configLoader.GenerateTagsPreviewByRules(addressRules);
            _logger.LogInformation("成功生成 {Count} 个标签预览", tags.Count);
            return tags.Cast<object>().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按规则生成标签失败");
            throw;
        }
    }

    public bool MergeToCsv(IEnumerable<object> scannedTags, string? backupSuffix = null)
    {
        var tags = scannedTags.Cast<ScannedTag>();
        _logger.LogInformation("正在合并 {Count} 个标签到 tags.csv...", tags.Count());
        try
        {
            var success = _configLoader.MergeTagsToCsv(tags, backupSuffix);
            if (success)
            {
                _logger.LogInformation("标签合并成功，已自动备份原文件");
            }
            else
            {
                _logger.LogWarning("标签合并失败");
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "合并标签时发生异常");
            return false;
        }
    }

    public bool GenerateAndMergeFromDbSchema(int dbNumber, string? backupSuffix = null)
    {
        var preview = PreviewFromDbSchema(dbNumber);
        if (preview.Count == 0)
        {
            _logger.LogWarning("DB{DbNumber} Schema 未生成任何标签，跳过合并", dbNumber);
            return false;
        }
        return MergeToCsv(preview, backupSuffix);
    }

    public bool GenerateAndMergeFromRules(object rules, string? backupSuffix = null)
    {
        var preview = PreviewFromRules(rules);
        if (preview.Count == 0)
        {
            _logger.LogWarning("规则未生成任何标签，跳过合并");
            return false;
        }
        return MergeToCsv(preview, backupSuffix);
    }

    public async Task<bool> TestS7ConnectionAsync(string ip, int port, int rack, int slot)
    {
        _logger.LogInformation("正在测试 S7 连接: {Ip}:{Port}, Rack={Rack}, Slot={Slot}", ip, port, rack, slot);
        try
        {
            var scanner = new S7AddressScanner();
            var result = await scanner.TestConnectionAsync(ip, port, rack, slot);
            if (result)
            {
                _logger.LogInformation("S7 连接测试成功");
            }
            else
            {
                _logger.LogWarning("S7 连接测试失败");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S7 连接测试异常");
            return false;
        }
    }
}
