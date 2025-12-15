using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;
using HslCommunication;
using HslCommunication.Profinet.Siemens;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

public class SiemensS7Driver : IDriver
{
    private DeviceConfig? _config;
    private bool _isConnected;
    private SiemensS7Net? _client;

    public bool IsConnected => _isConnected;

    public void Initialize(DeviceConfig config)
    {
        _config = config;
    }

    public void ValidateConfig(DeviceConfig config)
    {
        if (!config.Options.ContainsKey("IpAddress"))
            throw new ArgumentException("IpAddress is required for SiemensS7Driver");
        
        if (!config.Options.ContainsKey("Port"))
            throw new ArgumentException("Port is required for SiemensS7Driver");
    }

    public Task ConnectAsync()
    {
        if (_config == null) throw new InvalidOperationException("Driver not initialized");

        var ip = _config.Options.TryGetValue("IpAddress", out var ipObj) ? ipObj?.ToString() : null;
        var port = _config.Options.TryGetValue("Port", out var portObj) && int.TryParse(portObj?.ToString(), out var p) ? p : 102;
        var rack = _config.Options.TryGetValue("Rack", out var rackObj) && int.TryParse(rackObj?.ToString(), out var r) ? r : 0;
        var slot = _config.Options.TryGetValue("Slot", out var slotObj) && int.TryParse(slotObj?.ToString(), out var s) ? s : 1;

        if (string.IsNullOrWhiteSpace(ip)) throw new ArgumentException("IpAddress is required for SiemensS7Driver");

        _client = new SiemensS7Net(SiemensPLCS.S1200, ip)
        {
            Port = port,
            Rack = (byte)rack,
            Slot = (byte)slot,
            ConnectTimeOut = 5000
        };

        var res = _client.ConnectServer();
        _isConnected = res.IsSuccess;
        if (!_isConnected)
        {
            throw new InvalidOperationException($"S7 connect failed: {res.Message}");
        }
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        try
        {
            _client?.ConnectClose();
        }
        finally
        {
            _isConnected = false;
            _client = null;
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
    }

    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<Tag> tags)
    {
        if (_client == null || !_isConnected) throw new InvalidOperationException("S7 not connected");
        var dict = new Dictionary<string, object?>();

        foreach (var tag in tags)
        {
            if (!S7AddressParser.TryParse(tag.Address, out var addr))
            {
                dict[tag.Name] = null;
                continue;
            }

            try
            {
                // String: use ArrayLength as length parameter
                if (tag.DataType == TagDataType.String)
                {
                    var len = Math.Max(1, (int)tag.ArrayLength);
                    var r = _client.ReadString($"DB{addr.Db}.{addr.Offset}", (ushort)len);
                    dict[tag.Name] = r.IsSuccess ? r.Content : null;
                    continue;
                }

                // Arrays for numeric types: sequential offsets by type size
                int count = Math.Max(1, (int)tag.ArrayLength);
                if (count > 1 && addr.Kind != S7AddressKind.DBX)
                {
                    dict[tag.Name] = ReadArray(_client, tag.DataType, addr, count);
                    continue;
                }

                dict[tag.Name] = ReadScalar(_client, tag.DataType, addr);
            }
            catch
            {
                dict[tag.Name] = null;
            }
        }

        return Task.FromResult(dict);
    }

    public Task WriteTagAsync(Tag tag, object value)
    {
        if (_client == null || !_isConnected) throw new InvalidOperationException("S7 not connected");
        if (!S7AddressParser.TryParse(tag.Address, out var addr)) return Task.CompletedTask;

        // String: ArrayLength作为长度
        if (tag.DataType == TagDataType.String)
        {
            var s = Convert.ToString(value) ?? string.Empty;
            var len = Math.Max(1, (int)tag.ArrayLength);
            _client.Write($"DB{addr.Db}.{addr.Offset}", s.PadRight(len).Substring(0, len));
            return Task.CompletedTask;
        }

        switch (tag.DataType)
        {
            case TagDataType.Boolean:
                _client.Write($"DB{addr.Db}.{addr.Offset}.{addr.Bit}", Convert.ToBoolean(value));
                break;
            case TagDataType.Int16:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToInt16(value));
                break;
            case TagDataType.UInt16:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToUInt16(value));
                break;
            case TagDataType.Int32:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToInt32(value));
                break;
            case TagDataType.UInt32:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToUInt32(value));
                break;
            case TagDataType.Int64:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToInt64(value));
                break;
            case TagDataType.UInt64:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToUInt64(value));
                break;
            case TagDataType.Float:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToSingle(value));
                break;
            case TagDataType.Double:
                _client.Write($"DB{addr.Db}.{addr.Offset}", Convert.ToDouble(value));
                break;
        }

        return Task.CompletedTask;
    }

    private static object? ReadScalar(SiemensS7Net client, TagDataType type, S7Address addr)
    {
        switch (type)
        {
            case TagDataType.Boolean:
                {
                    var r = client.ReadBool($"DB{addr.Db}.{addr.Offset}.{addr.Bit}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.Byte:
                {
                    var r = client.ReadByte($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.Int16:
                {
                    var r = client.ReadInt16($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.UInt16:
                {
                    var r = client.ReadUInt16($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.Int32:
                {
                    var r = client.ReadInt32($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.UInt32:
                {
                    var r = client.ReadUInt32($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.Int64:
                {
                    var r = client.ReadInt64($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.UInt64:
                {
                    var r = client.ReadUInt64($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.Float:
                {
                    var r = client.ReadFloat($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.Double:
                {
                    var r = client.ReadDouble($"DB{addr.Db}.{addr.Offset}");
                    return r.IsSuccess ? r.Content : null;
                }
            case TagDataType.String:
                // handled outside with length
                return null;
            default:
                return null;
        }
    }

    private static object ReadArray(SiemensS7Net client, TagDataType type, S7Address addr, int count)
    {
        int size = type switch
        {
            TagDataType.Byte => 1,
            TagDataType.Int16 or TagDataType.UInt16 => 2,
            TagDataType.Int32 or TagDataType.UInt32 or TagDataType.Float => 4,
            TagDataType.Int64 or TagDataType.UInt64 or TagDataType.Double => 8,
            _ => 2
        };

        switch (type)
        {
            case TagDataType.Int16:
                {
                    var arr = new short[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadInt16($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.UInt16:
                {
                    var arr = new ushort[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadUInt16($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.Int32:
                {
                    var arr = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadInt32($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.UInt32:
                {
                    var arr = new uint[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadUInt32($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.Float:
                {
                    var arr = new float[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadFloat($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.Int64:
                {
                    var arr = new long[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadInt64($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.UInt64:
                {
                    var arr = new ulong[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadUInt64($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            case TagDataType.Double:
                {
                    var arr = new double[count];
                    for (int i = 0; i < count; i++)
                    {
                        var r = client.ReadDouble($"DB{addr.Db}.{addr.Offset + i * size}");
                        arr[i] = r.IsSuccess ? r.Content : default;
                    }
                    return arr;
                }
            default:
                return Array.Empty<object>();
        }
    }
}
