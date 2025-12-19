using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

/// <summary>
/// Siemens S7 驱动的标签扩展方法
/// </summary>
public static class SiemensS7TagExtensions
{
    /// <summary>
    /// 获取 S7 DB 编号
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 0）</param>
    /// <returns>DB 编号</returns>
    public static int GetS7DbNumber(this CommunicationTag tag, int defaultValue = 0)
    {
        return tag.GetExtendedProperty("S7DbNumber", defaultValue);
    }

    /// <summary>
    /// 设置 S7 DB 编号
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="dbNumber">DB 编号（0-65535）</param>
    public static void SetS7DbNumber(this CommunicationTag tag, int dbNumber)
    {
        if (dbNumber < 0 || dbNumber > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(dbNumber), "DB 编号必须在 0-65535 范围内");
        }
        tag.SetExtendedProperty("S7DbNumber", dbNumber);
    }

    /// <summary>
    /// 获取 S7 偏移量（字节偏移）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 0）</param>
    /// <returns>偏移量</returns>
    public static int GetS7Offset(this CommunicationTag tag, int defaultValue = 0)
    {
        return tag.GetExtendedProperty("S7Offset", defaultValue);
    }

    /// <summary>
    /// 设置 S7 偏移量（字节偏移）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="offset">偏移量（必须 >= 0）</param>
    public static void SetS7Offset(this CommunicationTag tag, int offset)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "偏移量必须 >= 0");
        }
        tag.SetExtendedProperty("S7Offset", offset);
    }

    /// <summary>
    /// 获取 S7 位偏移（用于 Boolean 类型，0-7）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 0）</param>
    /// <returns>位偏移</returns>
    public static int GetS7BitOffset(this CommunicationTag tag, int defaultValue = 0)
    {
        return tag.GetExtendedProperty("S7BitOffset", defaultValue);
    }

    /// <summary>
    /// 设置 S7 位偏移（用于 Boolean 类型）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="bitOffset">位偏移（0-7）</param>
    public static void SetS7BitOffset(this CommunicationTag tag, int bitOffset)
    {
        if (bitOffset < 0 || bitOffset > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(bitOffset), "位偏移必须在 0-7 范围内");
        }
        tag.SetExtendedProperty("S7BitOffset", bitOffset);
    }

    /// <summary>
    /// 获取 S7 区域类型（可选，用于更明确的配置）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>区域类型（DB/M/I/Q/T/C 等）</returns>
    public static string GetS7AreaType(this CommunicationTag tag, string defaultValue = "DB")
    {
        return tag.GetExtendedProperty("S7AreaType", defaultValue);
    }

    /// <summary>
    /// 设置 S7 区域类型
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="areaType">区域类型</param>
    public static void SetS7AreaType(this CommunicationTag tag, string areaType)
    {
        tag.SetExtendedProperty("S7AreaType", areaType);
    }

    /// <summary>
    /// 获取 S7 批量读取 ID
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <returns>批量读取 ID，如果不存在则返回 null</returns>
    public static int? GetS7BatchReadingId(this CommunicationTag tag)
    {
        if (tag.ExtendedProperties.TryGetValue("S7BatchReadingId", out var val) && val is int i)
        {
            return i;
        }
        return null;
    }
}
