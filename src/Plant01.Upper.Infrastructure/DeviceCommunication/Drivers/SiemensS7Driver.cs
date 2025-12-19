using HslCommunication.Profinet.Siemens;

using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;
using Plant01.Upper.Infrastructure.DeviceCommunication.DeviceAddressing;
using Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;
using Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;
using System.ComponentModel.DataAnnotations;

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
        // 使用强类型配置和自动验证
        var driverConfig = config.GetAndValidateDriverConfig<SiemensS7Config>();
        
        // 验证已通过 DataAnnotations 自动完成
        // 如需额外的业务验证,可在此添加
    }

    public Task ConnectAsync()
    {
        if (_config == null) throw new InvalidOperationException("驱动程序未初始化");

        // 使用强类型配置类
        var driverConfig = _config.GetDriverConfig<SiemensS7Config>();

        _client = new SiemensS7Net(SiemensPLCS.S1200, driverConfig.IpAddress)
        {
            Port = driverConfig.Port,
            Rack = (byte)driverConfig.Rack,
            Slot = (byte)driverConfig.Slot,
            ConnectTimeOut = driverConfig.ConnectTimeout
        };

        var res = _client.ConnectServer();
        _isConnected = res.IsSuccess;
        if (!_isConnected)
        {
            throw new InvalidOperationException($"S7 连接失败: {res.Message}");
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

    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        if (_client == null || !_isConnected) throw new InvalidOperationException("S7 未连接");
        var dict = new Dictionary<string, object?>();

        foreach (var tagObj in tags)
        {
            var tag = tagObj as CommunicationTag;
            if (tag == null) continue;
            
            if (!S7AddressParser.TryParse(tag.Address, out var addr))
            {
                dict[tag.Name] = null;
                continue;
            }

            try
            {
                // 字符串：使用 ArrayLength 作为长度参数
                if (tag.DataType == TagDataType.String)
                {
                    var len = Math.Max(1, (int)tag.ArrayLength);
                    var r = _client.ReadString($"DB{addr.Db}.{addr.Offset}", (ushort)len);
                    dict[tag.Name] = r.IsSuccess ? r.Content : null;
                    continue;
                }

                // 数值类型数组：按类型大小顺序偏移
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

    public Task WriteTagAsync(object tagObj, object value)
    {
        var tag = tagObj as CommunicationTag;
        if (tag == null) throw new ArgumentException("Invalid tag type");
        
        if (_client == null || !_isConnected) throw new InvalidOperationException("S7 未连接");
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

    private static object? ReadScalar(SiemensS7Net client, Models.TagDataType type, S7Address addr)
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
                // 在外部处理长度
                return null;
            default:
                return null;
        }
    }

    private static object ReadArray(SiemensS7Net client, Models.TagDataType type, S7Address addr, int count)
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
