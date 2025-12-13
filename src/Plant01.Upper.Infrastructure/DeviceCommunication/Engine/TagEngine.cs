using System.Collections.Concurrent;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

/// <summary>
/// 标签引擎
/// </summary>
public class TagEngine
{
    // 线程安全的字典，用于 O(1) 查找
    private readonly ConcurrentDictionary<string, Tag> _tags = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterTag(Tag tag)
    {
        _tags[tag.Name] = tag;
    }

    public void Clear()
    {
        _tags.Clear();
    }

    public Tag? GetTag(string tagName)
    {
        _tags.TryGetValue(tagName, out var tag);
        return tag;
    }

    public IEnumerable<Tag> GetAllTags()
    {
        return _tags.Values;
    }
}
