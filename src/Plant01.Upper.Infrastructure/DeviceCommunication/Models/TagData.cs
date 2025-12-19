namespace Plant01.Upper.Infrastructure.DeviceCommunication.Models;

/// <summary>
/// 标签数据结构体（用于快照和传递）
/// </summary>
public readonly struct TagData
{
    /// <summary>
    /// 获取数据值
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// 获取数据质量
    /// </summary>
    public TagQuality Quality { get; }

    /// <summary>
    /// 获取时间戳
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// 初始化标签数据
    /// </summary>
    public TagData(object? value, TagQuality quality, DateTime timestamp)
    {
        Value = value;
        Quality = quality;
        Timestamp = timestamp;
    }

    /// <summary>
    /// 获取指定类型的值，如果质量不佳或转换失败则返回默认值
    /// </summary>
    public T GetValue<T>(T defaultValue = default)
    {
        if (Value == null || Quality != TagQuality.Good)
        {
            return defaultValue;
        }

        try
        {
            if (Value is T tValue)
            {
                return tValue;
            }

            var targetType = typeof(T);

            if (Nullable.GetUnderlyingType(targetType) != null)
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            if (targetType == null) return defaultValue;

            return (T)Convert.ChangeType(Value, targetType);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 判断数据是否有效（质量良好且值不为空）
    /// </summary>
    public bool IsValid => Quality == TagQuality.Good && Value != null;

    /// <summary>
    /// 判断数据是否已过期
    /// </summary>
    public bool IsExpired(int timeout)
    {
        return (DateTime.Now - Timestamp).TotalSeconds > timeout;
    }
}
