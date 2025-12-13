namespace Plant01.Upper.Domain.Models.DeviceCommunication;

public class ChannelConfig
{
    public string Name { get; set; } = string.Empty;
    public string DriverType { get; set; } = string.Empty; // e.g., "SiemensS7", "Modbus"
    public bool Enabled { get; set; } = true;
    
    // Common settings for the channel/driver if any
    public Dictionary<string, object> Options { get; set; } = new();

    public List<DeviceConfig> Devices { get; set; } = new();
}
