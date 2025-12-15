namespace Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

public class AddressRules
{
    public required int DbNumber { get; init; }
    public required string NameTemplate { get; init; } // e.g., "Tag_{Index}"
    public int StartOffset { get; init; } = 0; // bytes
    public int Stride { get; init; } = 4; // bytes per item
    public int Count { get; init; } = 1; // number of tags
    public string DataType { get; init; } = "Int32"; // Boolean, Int16, Int32, Float, String
    public int StringLength { get; init; } = 0; // for strings
    public int BitIndex { get; init; } = -1; // for DBX bit addressing
}
