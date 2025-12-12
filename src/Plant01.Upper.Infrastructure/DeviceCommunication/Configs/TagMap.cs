using CsvHelper.Configuration;
using Plant01.Upper.Domain.Models.DeviceCommunication;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Configs;

public class TagMap : ClassMap<Tag>
{
    public TagMap()
    {
        Map(m => m.Name).Name("TagName");
        Map(m => m.Address).Name("Address");
        Map(m => m.DataType).Name("DataType");
        Map(m => m.DeviceCode).Name("DeviceCode");
        Map(m => m.DriverCode).Name("DriverCode");
        Map(m => m.Length).Name("Length").Default(0);
        
        // Custom mapping for IsWriteOnly based on RW column
        Map(m => m.IsWriteOnly).Convert(args => 
        {
            var rw = args.Row.GetField("RW");
            return string.Equals(rw, "WRITEONLY", StringComparison.OrdinalIgnoreCase);
        });
    }
}
