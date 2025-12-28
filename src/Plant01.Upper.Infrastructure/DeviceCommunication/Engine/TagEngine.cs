using System.Collections.Concurrent;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

/// <summary>
/// 标签引擎（Infrastructure 层 - 使用 CommunicationTag）
/// </summary>
public class TagEngine
{
    // 线程安全的字典，提供 O(1) 查询
    private readonly ConcurrentDictionary<string, CommunicationTag> _tags = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterTag(CommunicationTag tag)
    {
        _tags[tag.Code] = tag;
    }

    public void Clear()
    {
        _tags.Clear();
    }

    public CommunicationTag? GetTag(string tagName)
    {
        _tags.TryGetValue(tagName, out var tag);
        return tag;
    }

    public IEnumerable<CommunicationTag> GetAllTags()
    {
        return _tags.Values;
    }
}
