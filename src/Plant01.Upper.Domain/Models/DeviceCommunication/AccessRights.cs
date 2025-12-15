namespace Plant01.Upper.Domain.Models.DeviceCommunication;

[Flags]
public enum AccessRights
{
    None = 0,
    Read = 1,
    Write = 2,
    ReadWrite = Read | Write
}
