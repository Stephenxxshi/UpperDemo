using CsvHelper;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Parsers;

/// <summary>
/// 西门子 S7 驱动标签解析器
/// </summary>
public class S7TagParser : IDriverTagParser
{
    public string DriverType => "SiemensS7Tcp";

    public Dictionary<string, object> ParseExtendedProperties(IReaderRow row)
    {
        var props = new Dictionary<string, object>();

        // 解析 S7AreaType
        if (row.TryGetField("S7AreaType", out string? areaType) && !string.IsNullOrEmpty(areaType))
        {
            props["S7AreaType"] = areaType;
        }

        // 解析 S7DbNumber
        if (row.TryGetField("S7DbNumber", out string? dbNumStr) && int.TryParse(dbNumStr, out int dbNum))
        {
            props["S7DbNumber"] = dbNum;
        }

        // 解析 S7Offset
        if (row.TryGetField("S7Offset", out string? offsetStr) && int.TryParse(offsetStr, out int offset))
        {
            props["S7Offset"] = offset;
        }

        // 解析 S7BitOffset
        if (row.TryGetField("S7BitOffset", out string? bitOffsetStr) && int.TryParse(bitOffsetStr, out int bitOffset))
        {
            props["S7BitOffset"] = bitOffset;
        }

        // 解析 S7BatchReadingId
        if (row.TryGetField("S7BatchReadingId", out string? batchIdStr) && int.TryParse(batchIdStr, out int batchId))
        {
            props["S7BatchReadingId"] = batchId;
        }

        return props;
    }
}
