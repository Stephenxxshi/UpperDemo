namespace Plant01.Upper.Domain.Models;

/// <summary>
/// 标签质量枚举（Domain 层）
/// </summary>
public enum TagQuality
{
    /// <summary>
    /// 良好
    /// </summary>
    Good = 192,

    /// <summary>
    /// 错误
    /// </summary>
    Bad = 0,

    /// <summary>
    /// 不确定
    /// </summary>
    Uncertain = 64
}

/// <summary>
/// 标签值数据结构（Domain 层 - 纯领域模型，不包含通信层概念）
/// </summary>
public readonly struct TagValue
{
    /// <summary>
    /// 获取标签名称
    /// </summary>
    public string Name { get; }

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
    /// 初始化标签值
    /// </summary>
    public TagValue(string name, object? value, TagQuality quality, DateTime timestamp)
    {
        Name = name;
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
    public bool IsExpired(int timeoutSeconds)
    {
        return (DateTime.Now - Timestamp).TotalSeconds > timeoutSeconds;
    }

    /// <summary>
    /// 转换为字符串表示
    /// </summary>
    public override string ToString()
    {
        return $"{Name}: {Value} ({Quality}) @ {Timestamp:yyyy-MM-dd HH:mm:ss}";
    }
}
