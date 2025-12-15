namespace Plant01.Upper.Domain.Models.DeviceCommunication;

public enum TagQuality
{
    Good = 192,
    Bad = 0,
    Uncertain = 64
}

public readonly struct TagData
{
    public object? Value { get; }
    public TagQuality Quality { get; }
    public DateTime Timestamp { get; }

    public TagData(object? value, TagQuality quality, DateTime timestamp)
    {
        Value = value;
        Quality = quality;
        Timestamp = timestamp;
    }

    public T GetValue<T>(T defaultValue = default)
    {
        if (Value == null || Quality != TagQuality.Good)
        {
            return defaultValue;
        }

        try
        {
            // Handle direct cast
            if (Value is T tValue)
            {
                return tValue;
            }

            // Handle conversion
            var targetType = typeof(T);
            
            // Handle Nullable types
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
}

public class Tag
{
    private object? _value;
    private TagQuality _quality;
    private DateTime _timestamp;
    private readonly object _lock = new();

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public TagDataType DataType { get; set; } = TagDataType.Int16;
    public ushort ArrayLength { get; set; } = 1;
    
    // Length in bytes for String type, or data size for other purposes if needed
    public int DataSize { get; set; } 

    public string DeviceName { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    
    public AccessRights AccessRights { get; set; } = AccessRights.ReadWrite;

    public Tag()
    {
        _quality = TagQuality.Bad;
        _timestamp = DateTime.MinValue;
    }

    public Tag(string name, string address, TagDataType dataType, string deviceName, string channelName, AccessRights accessRights, ushort arrayLength = 1)
    {
        Name = name;
        Address = address;
        DataType = dataType;
        DeviceName = deviceName;
        ChannelName = channelName;
        AccessRights = accessRights;
        ArrayLength = arrayLength;
        
        _quality = TagQuality.Bad;
        _timestamp = DateTime.MinValue;
    }

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

    public TagData GetSnapshot()
    {
        lock (_lock)
        {
            return new TagData(_value, _quality, _timestamp);
        }
    }
}
