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
}

public class Tag
{
    private object? _value;
    private TagQuality _quality;
    private DateTime _timestamp;
    private readonly object _lock = new();

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string DeviceCode { get; set; } = string.Empty;
    public string DriverCode { get; set; } = string.Empty;
    public int Length { get; set; }
    public bool IsWriteOnly { get; set; }

    public Tag()
    {
        _quality = TagQuality.Bad;
        _timestamp = DateTime.MinValue;
    }

    public Tag(string name, string address, string dataType, string deviceCode, string driverCode, bool isWriteOnly, int length = 0)
    {
        Name = name;
        Address = address;
        DataType = dataType;
        DeviceCode = deviceCode;
        DriverCode = driverCode;
        IsWriteOnly = isWriteOnly;
        Length = length;
        
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
