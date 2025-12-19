using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

/// <summary>
/// OPC UA 驱动的标签扩展方法
/// </summary>
public static class OpcUaTagExtensions
{
    /// <summary>
    /// 获取 OPC UA NodeId
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为空字符串）</param>
    /// <returns>NodeId 字符串表示</returns>
    public static string GetOpcUaNodeId(this CommunicationTag tag, string defaultValue = "")
    {
        return tag.GetExtendedProperty("OpcUaNodeId", defaultValue);
    }

    /// <summary>
    /// 设置 OPC UA NodeId
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="nodeId">NodeId（如 "ns=2;s=Device1.Temperature" 或 "i=2259"）</param>
    public static void SetOpcUaNodeId(this CommunicationTag tag, string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            throw new ArgumentException("NodeId 不能为空", nameof(nodeId));
        }
        tag.SetExtendedProperty("OpcUaNodeId", nodeId);
    }

    /// <summary>
    /// 获取 OPC UA NamespaceIndex（命名空间索引）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 0）</param>
    /// <returns>命名空间索引</returns>
    public static ushort GetOpcUaNamespaceIndex(this CommunicationTag tag, ushort defaultValue = 0)
    {
        return tag.GetExtendedProperty("OpcUaNamespaceIndex", defaultValue);
    }

    /// <summary>
    /// 设置 OPC UA NamespaceIndex
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="namespaceIndex">命名空间索引</param>
    public static void SetOpcUaNamespaceIndex(this CommunicationTag tag, ushort namespaceIndex)
    {
        tag.SetExtendedProperty("OpcUaNamespaceIndex", namespaceIndex);
    }

    /// <summary>
    /// 获取 OPC UA 浏览路径（可选，用于层级化访问）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>浏览路径</returns>
    public static string GetOpcUaBrowsePath(this CommunicationTag tag, string defaultValue = "")
    {
        return tag.GetExtendedProperty("OpcUaBrowsePath", defaultValue);
    }

    /// <summary>
    /// 设置 OPC UA 浏览路径
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="browsePath">浏览路径（如 "/Objects/Device1/Temperature"）</param>
    public static void SetOpcUaBrowsePath(this CommunicationTag tag, string browsePath)
    {
        tag.SetExtendedProperty("OpcUaBrowsePath", browsePath);
    }

    /// <summary>
    /// 获取 OPC UA 采样间隔（毫秒）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 1000ms）</param>
    /// <returns>采样间隔（毫秒）</returns>
    public static int GetOpcUaSamplingInterval(this CommunicationTag tag, int defaultValue = 1000)
    {
        return tag.GetExtendedProperty("OpcUaSamplingInterval", defaultValue);
    }

    /// <summary>
    /// 设置 OPC UA 采样间隔
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="samplingInterval">采样间隔（毫秒，必须 > 0）</param>
    public static void SetOpcUaSamplingInterval(this CommunicationTag tag, int samplingInterval)
    {
        if (samplingInterval <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(samplingInterval), "采样间隔必须 > 0");
        }
        tag.SetExtendedProperty("OpcUaSamplingInterval", samplingInterval);
    }
}
