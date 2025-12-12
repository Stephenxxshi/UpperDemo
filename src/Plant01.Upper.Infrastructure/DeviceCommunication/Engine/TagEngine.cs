using System.Collections.Concurrent;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Engine;

public class TagEngine
{
    // Thread-safe dictionary for O(1) lookup
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
