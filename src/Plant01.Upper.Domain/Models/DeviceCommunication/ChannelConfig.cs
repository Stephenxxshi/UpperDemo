namespace Plant01.Upper.Domain.Models.DeviceCommunication;

public class ChannelConfig
{
    public string Name { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty; // e.g., "SiemensS7", "Modbus"
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public int ScanRate { get; set; } = 100; // ms
    public bool Enable { get; set; } = true;
}
