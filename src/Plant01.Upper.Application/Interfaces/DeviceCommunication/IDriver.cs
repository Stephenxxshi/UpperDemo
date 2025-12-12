using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Application.Interfaces.DeviceCommunication;

public interface IDriver : IDisposable
{
    bool IsConnected { get; }
    Task ConnectAsync();
    Task DisconnectAsync();
    
    // 批量读取，返回 TagName -> Value 字典
    Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<Tag> tags);
    
    Task WriteTagAsync(Tag tag, object value);
}
