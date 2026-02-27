using CsvHelper.Configuration;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Configs;

public class TagMap : ClassMap<CommunicationTag>
{
    public TagMap()
    {
        Map(m => m.Code).Name("TagCode");
        Map(m => m.Address).Name("Address");

        Map(m => m.DataType).Name("DataType").Convert(args => 
        {
            var typeStr = args.Row.GetField("DataType");
            if (Enum.TryParse<TagDataType>(typeStr, true, out var result))
            {
                return result;
            }
            // 处理DataType类型
            if (string.Equals(typeStr, "Short", StringComparison.OrdinalIgnoreCase)) return TagDataType.Int16;
            if (string.Equals(typeStr, "UShort", StringComparison.OrdinalIgnoreCase)) return TagDataType.UInt16;
            if (string.Equals(typeStr, "Int", StringComparison.OrdinalIgnoreCase)) return TagDataType.Int32;
            if (string.Equals(typeStr, "UInt", StringComparison.OrdinalIgnoreCase)) return TagDataType.UInt32;
            if (string.Equals(typeStr, "Bool", StringComparison.OrdinalIgnoreCase)) return TagDataType.Boolean;
            if (string.Equals(typeStr, "Float", StringComparison.OrdinalIgnoreCase)) return TagDataType.Float;
            
            return TagDataType.Int16; // Default
        });

        Map(m => m.DeviceCode).Name("DeviceCode");
        Map(m => m.ChannelCode).Name("ChannelCode");

        Map(m => m.ArrayLength).Name("Length").Default((ushort)1).Convert(args => 
        {
            var lenStr = args.Row.GetField("Length");
            if (ushort.TryParse(lenStr, out var len) && len > 0)
            {
                return len;
            }
            return (ushort)1;
        });
        
        Map(m => m.AccessRights).Name("RW").Convert(args => 
        {
            var rw = args.Row.GetField("RW");
            if (string.Equals(rw, "R", StringComparison.OrdinalIgnoreCase)) return AccessRights.Read;
            if (string.Equals(rw, "W", StringComparison.OrdinalIgnoreCase)) return AccessRights.Write;
            return AccessRights.ReadWrite;
        });
    }
}
