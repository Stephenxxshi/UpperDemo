using Plant01.Upper.Application.Models;
using Plant01.Upper.Domain.Models;

namespace Plant01.Upper.Application.Interfaces.DeviceCommunication;

/// <summary>
/// 设备通信服务接口（Application 层 - 只使用领域模型）
/// </summary>
public interface IDeviceCommunicationService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取标签值（返回领域模型）
    /// </summary>
    TagValue GetTagValue(string tagName);
    
    /// <summary>
    /// 获取指定类型的标签值
    /// </summary>
    T GetTagValue<T>(string tagName, T defaultValue = default);
    
    /// <summary>
    /// 写入标签值
    /// </summary>
    Task WriteTagAsync(string tagName, object value);
    
    /// <summary>
    /// 标签值变化事件
    /// </summary>
    event EventHandler<TagChangeEventArgs>? TagChanged;
}

/// <summary>
/// 标签变化事件参数
/// </summary>
public class TagChangeEventArgs : EventArgs
{
    public string TagCode { get; }
    public TagValue NewValue { get; }
    public TriggerSourceType TriggerSourceType { get; set; }

    public TagChangeEventArgs(string tagCode, TagValue newValue,TriggerSourceType triggerSource = TriggerSourceType.System)
    {
        TagCode = tagCode;
        NewValue = newValue;
        TriggerSourceType = triggerSource;
    }
}
