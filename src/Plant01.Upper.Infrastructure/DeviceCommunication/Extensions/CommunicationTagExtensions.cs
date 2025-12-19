using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

/// <summary>
/// CommunicationTag 通用扩展方法
/// 提供访问扩展属性的基础方法
/// 驱动特定的扩展方法请参考对应的扩展类：
/// - ModbusTagExtensions: Modbus 驱动
/// - SiemensS7TagExtensions: Siemens S7 驱动
/// - OpcUaTagExtensions: OPC UA 驱动
/// </summary>
public static class CommunicationTagExtensions
{
    /// <summary>
    /// 获取扩展属性值
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    /// <param name="tag">标签对象</param>
    /// <param name="key">属性键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>属性值或默认值</returns>
    public static T GetExtendedProperty<T>(this CommunicationTag tag, string key, T defaultValue = default)
    {
        if (tag.ExtendedProperties == null || !tag.ExtendedProperties.ContainsKey(key))
        {
            return defaultValue;
        }

        var value = tag.ExtendedProperties[key];
        if (value is T typedValue)
        {
            return typedValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 设置扩展属性值
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="key">属性键</param>
    /// <param name="value">属性值</param>
    public static void SetExtendedProperty(this CommunicationTag tag, string key, object value)
    {
        tag.ExtendedProperties ??= new Dictionary<string, object>();
        tag.ExtendedProperties[key] = value;
    }

    /// <summary>
    /// 批量设置扩展属性
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="properties">属性字典</param>
    public static void SetExtendedProperties(this CommunicationTag tag, Dictionary<string, object> properties)
    {
        tag.ExtendedProperties ??= new Dictionary<string, object>();
        foreach (var kvp in properties)
        {
            tag.ExtendedProperties[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// 判断是否包含指定扩展属性
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="key">属性键</param>
    /// <returns>是否包含</returns>
    public static bool HasExtendedProperty(this CommunicationTag tag, string key)
    {
        return tag.ExtendedProperties?.ContainsKey(key) ?? false;
    }
}
