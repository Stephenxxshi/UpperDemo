namespace Plant01.Upper.Infrastructure.DeviceCommunication.Models;

/// <summary>
/// 访问权限枚举
/// </summary>
[Flags]
public enum AccessRights
{
    /// <summary>
    /// 无权限
    /// </summary>
    None = 0,

    /// <summary>
    /// 只读
    /// </summary>
    Read = 1,

    /// <summary>
    /// 只写
    /// </summary>
    Write = 2,

    /// <summary>
    /// 读写
    /// </summary>
    ReadWrite = Read | Write
}

/// <summary>
/// 标签数据类型枚举
/// </summary>
public enum TagDataType
{
    Boolean,
    Byte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Double,
    String,
    DateTime
}

/// <summary>
/// 标签质量枚举
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

/// <summary>
/// 通信标签类（Infrastructure 层专用，包含通信相关属性）
/// </summary>
public class CommunicationTag
{
    private object? _value;
    private TagQuality _quality;
    private DateTime _timestamp;
    private readonly object _lock = new();

    /// <summary>
    /// 获取或设置标签名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置标签描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置标签地址（PLC地址）
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置数据类型
    /// </summary>
    public TagDataType DataType { get; set; } = TagDataType.Int16;

    /// <summary>
    /// 获取或设置数组长度
    /// </summary>
    public ushort ArrayLength { get; set; } = 1;
    
    /// <summary>
    /// 获取或设置数据大小（字节长度，用于字符串类型）
    /// </summary>
    public int DataSize { get; set; } 

    /// <summary>
    /// 获取或设置设备名称（通信层概念）
    /// </summary>
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置通道名称（通信层概念）
    /// </summary>
    public string ChannelCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 获取或设置访问权限
    /// </summary>
    public AccessRights AccessRights { get; set; } = AccessRights.ReadWrite;

    /// <summary>
    /// 获取当前值（只读，线程安全）
    /// </summary>
    public object? CurrentValue
    {
        get
        {
            lock (_lock)
            {
                return _value;
            }
        }
    }

    /// <summary>
    /// 获取当前质量（只读，线程安全）
    /// </summary>
    public TagQuality CurrentQuality
    {
        get
        {
            lock (_lock)
            {
                return _quality;
            }
        }
    }

    /// <summary>
    /// 获取当前时间戳（只读，线程安全）
    /// </summary>
    public DateTime CurrentTimestamp
    {
        get
        {
            lock (_lock)
            {
                return _timestamp;
            }
        }
    }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CommunicationTag()
    {
        _quality = TagQuality.Bad;
        _timestamp = DateTime.MinValue;
    }

    /// <summary>
    /// 带参数的构造函数
    /// </summary>
    public CommunicationTag(string name, string address, TagDataType dataType, string deviceName, string channelName, AccessRights accessRights, ushort arrayLength = 1)
    {
        Name = name;
        Address = address;
        DataType = dataType;
        DeviceCode = deviceName;
        ChannelCode = channelName;
        AccessRights = accessRights;
        ArrayLength = arrayLength;
        
        _quality = TagQuality.Bad;
        _timestamp = DateTime.MinValue;
    }

    /// <summary>
    /// 更新标签值和质量
    /// </summary>
    public bool Update(object? value, TagQuality quality)
    {
        lock (_lock)
        {
            bool changed = !Equals(_value, value) || _quality != quality;
            _value = value;
            _quality = quality;
            _timestamp = DateTime.Now;
            return changed;
        }
    }

    /// <summary>
    /// 仅更新标签值（质量自动设置为Good）
    /// </summary>
    public bool UpdateValue(object? value)
    {
        return Update(value, TagQuality.Good);
    }

    /// <summary>
    /// 设置标签为错误状态
    /// </summary>
    public void SetBad()
    {
        Update(null, TagQuality.Bad);
    }

    /// <summary>
    /// 设置标签为不确定状态
    /// </summary>
    public void SetUncertain()
    {
        lock (_lock)
        {
            _quality = TagQuality.Uncertain;
            _timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// 获取标签数据的快照
    /// </summary>
    public TagData GetSnapshot()
    {
        lock (_lock)
        {
            return new TagData(_value, _quality, _timestamp);
        }
    }

    /// <summary>
    /// 获取指定类型的当前值
    /// </summary>
    public T GetValue<T>(T defaultValue = default)
    {
        return GetSnapshot().GetValue<T>(defaultValue);
    }

    /// <summary>
    /// 判断当前数据是否有效
    /// </summary>
    public bool IsValid()
    {
        lock (_lock)
        {
            return _quality == TagQuality.Good && _value != null;
        }
    }

    /// <summary>
    /// 判断数据是否已过期
    /// </summary>
    public bool IsExpired(int timeoutSeconds)
    {
        lock (_lock)
        {
            return (DateTime.Now - _timestamp).TotalSeconds > timeoutSeconds;
        }
    }
}
