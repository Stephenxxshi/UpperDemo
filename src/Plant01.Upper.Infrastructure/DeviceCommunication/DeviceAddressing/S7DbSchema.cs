using System.Collections.Generic;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

public class S7DbSchema
{
    public required int DbNumber { get; init; }
    public List<S7DbField> Fields { get; init; } = new();
}

public class S7DbField
{
    public required string Name { get; init; }
    public required string DataType { get; init; } // Boolean, Int16, Int32, Float, String
    public int Offset { get; init; } // byte offset
    public int BitIndex { get; init; } = -1; // for Boolean in DBX
    public int Length { get; init; } = 1; // array length or string length
    public string? RW { get; init; } // optional read/write
}
