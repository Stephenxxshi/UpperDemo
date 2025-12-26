namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// 标签生成服务接口 (应用层不直接依赖基础设施)
/// 具体实现应在基础设施层
/// </summary>
public interface ITagGenerationService
{
    List<object> PreviewFromDbSchema(int dbNumber);
    List<object> PreviewFromRules(object rules);
    bool MergeToCsv(IEnumerable<object> scannedTags, string? backupSuffix = null);
    bool GenerateAndMergeFromDbSchema(int dbNumber, string? backupSuffix = null);
    bool GenerateAndMergeFromRules(object rules, string? backupSuffix = null);
    
    // 新增：从 TIA Portal DB 文件内容生成
    List<object> PreviewFromTiaDbFile(string content);
    bool GenerateAndMergeFromTiaDbFile(string content, string? backupSuffix = null);

    Task<bool> TestS7ConnectionAsync(string ip, int port, int rack, int slot);
}


