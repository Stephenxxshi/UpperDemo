
using Plant01.Upper.Application.Models.DeviceCommunication;

namespace Plant01.Upper.Application.Interfaces.DeviceCommunication;

/// <summary>
/// 驱动接口（注意：此接口在 Application 层定义，但实现在 Infrastructure 层）
/// 使用通信层的 CommunicationTag 类型（通过 dynamic 或在实现层转换）
/// </summary>
public interface IDriver : IDisposable
{
    bool IsConnected { get; }
    
    void Initialize(DeviceConfig config);
    void ValidateConfig(DeviceConfig config);

    Task ConnectAsync();
    Task DisconnectAsync();
    
    /// <summary>
    /// 批量读取标签（实现层使用 CommunicationTag）
    /// </summary>
    Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags);
    
    /// <summary>
    /// 写入标签（实现层使用 CommunicationTag）
    /// </summary>
    Task WriteTagAsync(object tag, object value);
}
