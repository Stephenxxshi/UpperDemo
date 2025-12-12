namespace Plant01.Upper.Application.Models.Devices;

public class CommunicationChannelBase
{
    public string Name { get; set; } = string.Empty;
    public string AliasName { get; set; } = string.Empty;
    public bool IsEnable { get; set; }
    public DriveType Drive{ get; set; }
    public string DriveModel { get; set; } = string.Empty;
}
