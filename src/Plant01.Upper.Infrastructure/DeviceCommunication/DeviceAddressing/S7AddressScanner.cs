using HslCommunication;
using HslCommunication.Profinet.Siemens;

using System.Globalization;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;

public class S7AddressScanner
{
    private SiemensS7Net? _client;

    public async Task<bool> TestConnectionAsync(string ip, int port, int rack, int slot)
    {
        try
        {
            _client = new SiemensS7Net(SiemensPLCS.S1200, ip)
            {
                Port = port,
                Rack = (byte)rack,
                Slot = (byte)slot,
                ConnectTimeOut = 5000
            };
            OperateResult connect = await Task.Run(() => _client.ConnectServer());
            if (!connect.IsSuccess) return false;
            _client.ConnectClose();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Scan via schema definition (preferred DB-structure mode)
    public Task<List<ScannedTag>> GenerateFromSchemaAsync(S7DbSchema schema)
    {
        var list = new List<ScannedTag>();
        foreach (var f in schema.Fields)
        {
            var address = NormalizeAddress(schema.DbNumber, f.DataType, f.Offset, f.BitIndex, f.Length);
            list.Add(new ScannedTag
            {
                TagName = f.Name,
                Address = address,
                DataType = f.DataType,
                Length = f.DataType.Equals("String", StringComparison.OrdinalIgnoreCase) ? f.Length : Math.Max(1, f.Length),
                RW = string.IsNullOrWhiteSpace(f.RW) ? "R" : f.RW!
            });
        }
        return Task.FromResult(list);
    }

    public Task<List<ScannedTag>> GenerateByRulesAsync(AddressRules rules)
    {
        var list = new List<ScannedTag>();
        for (int i = 0; i < rules.Count; i++)
        {
            var name = rules.NameTemplate.Replace("{Index}", i.ToString(CultureInfo.InvariantCulture));
            var offset = rules.StartOffset + i * rules.Stride;
            var address = NormalizeAddress(rules.DbNumber, rules.DataType, offset, rules.BitIndex, rules.StringLength);
            list.Add(new ScannedTag
            {
                TagName = name,
                Address = address,
                DataType = rules.DataType,
                Length = rules.DataType.Equals("String", StringComparison.OrdinalIgnoreCase) ? rules.StringLength : 1,
                RW = "R"
            });
        }
        return Task.FromResult(list);
    }

    private static string NormalizeAddress(int db, string dataType, int offset, int bitIndex, int strLen)
    {
        var dt = dataType.ToLowerInvariant();
        return dt switch
        {
            "boolean" => bitIndex >= 0 ? $"DB{db}.DBX{offset}.{bitIndex}" : $"DB{db}.DBX{offset}.0",
            "int16" => $"DB{db}.DBW{offset}",
            "int32" => $"DB{db}.DBD{offset}",
            "float" => $"DB{db}.DBD{offset}",
            "string" => strLen > 0 ? $"DB{db}.DBS{offset}[{strLen}]" : $"DB{db}.DBS{offset}",
            _ => $"DB{db}.DBD{offset}"
        };
    }
}

public class ScannedTag
{
    public required string TagName { get; init; }
    public required string Address { get; init; }
    public required string DataType { get; init; }
    public int Length { get; init; }
    public string RW { get; init; } = "R";
}
