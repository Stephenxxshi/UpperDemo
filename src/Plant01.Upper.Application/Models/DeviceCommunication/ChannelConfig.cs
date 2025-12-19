namespace Plant01.Upper.Application.Models.DeviceCommunication;

public class ChannelConfig
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public string DriverType { get; set; } = string.Empty; // e.g., "SiemensS7", "Modbus"
    public string DriveModel { get; set; } = string.Empty;  

    // Common settings for the channel/driver if any
    public Dictionary<string, object> Options { get; set; } = new();

    public List<DeviceConfig> Devices { get; set; } = new();
}
