using System.Text;
using HslCommunication;
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

    // 定义最大允许的字节间隙。
    // TCP 通讯的往返时间(RTT)通常远大于读取少量多余字节的开销。
    // 将间隙阈值调大（例如 200 字节），可以显著减少请求次数，提高总吞吐量。
    // 只要间隙内的数据在 PLC DB 块范围内，读取它们是安全的。
    private const int MaxByteGap = 500;

    public Task<Dictionary<string, object?>> ReadTagsAsync(IEnumerable<object> tags)
    {
        if (_client == null || !_isConnected) throw new InvalidOperationException("S7 未连接");
        var dict = new Dictionary<string, object?>();

        var commTags = tags.OfType<CommunicationTag>().ToList();
        
        // 1. 筛选出配置了 BatchId 的标签
        var batchTags = commTags.Where(t => t.GetS7BatchReadingId().HasValue).ToList();
        var singleTags = commTags.Where(t => !t.GetS7BatchReadingId().HasValue).ToList();

        // 2. 处理批量读取
        if (batchTags.Any())
        {
            var groups = batchTags.GroupBy(t => t.GetS7BatchReadingId()!.Value);
            foreach (var group in groups)
            {
                // 优化：智能拆分与读取
                ProcessBatchGroupSmart(group.ToList(), dict);
            }
        }

        // 3. 处理剩余的单独读取
        foreach (var tag in singleTags)
        {
            ReadSingleTag(tag, dict);
        }

        return Task.FromResult(dict);
    }

    private void ProcessBatchGroupSmart(List<CommunicationTag> tags, Dictionary<string, object?> dict)
    {
        if (!tags.Any()) return;

        // 预处理：按 DB 和 Area 分组，防止不同区域的标签混在同一个 BatchId
        // 使用元组作为 Key
        var areaGroups = tags.GroupBy(t => (Db: t.GetS7DbNumber(), Area: t.GetS7AreaType()));

        foreach (var areaGroup in areaGroups)
        {
            // 按偏移量排序
            var sortedTags = areaGroup.OrderBy(t => t.GetS7Offset()).ToList();
            
            // 智能拆分算法
            var currentBatch = new List<CommunicationTag>();
            int currentStart = -1;
            int currentEnd = -1;

            foreach (var tag in sortedTags)
            {
                int tagStart = tag.GetS7Offset();
                int tagLen = GetTagByteLength(tag);
                int tagEnd = tagStart + tagLen;

                if (currentBatch.Count == 0)
                {
                    currentBatch.Add(tag);
                    currentStart = tagStart;
                    currentEnd = tagEnd;
                }
                else
                {
                    // 检查间隙
                    if (tagStart - currentEnd > MaxByteGap)
                    {
                        // 间隙过大，执行上一批次
                        ExecuteBatchRead(currentBatch, areaGroup.Key.Db, currentStart, currentEnd, dict);
                        
                        // 开启新批次
                        currentBatch.Clear();
                        currentBatch.Add(tag);
                        currentStart = tagStart;
                        currentEnd = tagEnd;
                    }
                    else
                    {
                        // 间隙可接受，合并
                        currentBatch.Add(tag);
                        currentEnd = Math.Max(currentEnd, tagEnd);
                    }
                }
            }

            // 执行最后一批
            if (currentBatch.Count > 0)
            {
                ExecuteBatchRead(currentBatch, areaGroup.Key.Db, currentStart, currentEnd, dict);
            }
        }
    }

    private void ExecuteBatchRead(List<CommunicationTag> tags, int dbNumber, int startOffset, int endOffset, Dictionary<string, object?> dict)
    {
        ushort length = (ushort)(endOffset - startOffset);
        
        // 尝试批量读取
        // 假设都是 DB 块读取，如果需要支持 M/I/Q 区，需要根据 AreaType 调整指令
        var result = _client!.Read($"DB{dbNumber}.{startOffset}", length);

        if (result.IsSuccess)
        {
            // 读取成功，解析数据
            var content = result.Content;
            foreach (var tag in tags)
            {
                try 
                {
                    int relativeOffset = tag.GetS7Offset() - startOffset;
                    object? val = ParseTagValueFromBytes(tag, content, relativeOffset);
                    dict[tag.Name] = val;
                }
                catch
                {
                    dict[tag.Name] = null;
                }
            }
        }
        else
        {
            // 优化：降级策略 (Fallback)
            // 如果批量读取失败，尝试逐个读取，避免全军覆没
            foreach (var tag in tags)
            {
                ReadSingleTag(tag, dict);
            }
        }
    }

    private object? ParseTagValueFromBytes(CommunicationTag tag, byte[] buffer, int offset)
    {
        // Check bounds
        if (offset >= buffer.Length) return null;

        switch (tag.DataType)
        {
            case TagDataType.Boolean:
                // Bool needs bit offset
                int bitIndex = tag.GetS7BitOffset();
                return _client!.ByteTransform.TransByte(buffer, offset).GetBoolByIndex(bitIndex);
            
            case TagDataType.Byte:
                return _client!.ByteTransform.TransByte(buffer, offset);

            case TagDataType.Int16:
                return _client!.ByteTransform.TransInt16(buffer, offset);
                
            case TagDataType.UInt16:
                return _client!.ByteTransform.TransUInt16(buffer, offset);

            case TagDataType.Int32:
                return _client!.ByteTransform.TransInt32(buffer, offset);

            case TagDataType.UInt32:
                return _client!.ByteTransform.TransUInt32(buffer, offset);

            case TagDataType.Int64:
                return _client!.ByteTransform.TransInt64(buffer, offset);

            case TagDataType.UInt64:
                return _client!.ByteTransform.TransUInt64(buffer, offset);

            case TagDataType.Float:
                return _client!.ByteTransform.TransSingle(buffer, offset);

            case TagDataType.Double:
                return _client!.ByteTransform.TransDouble(buffer, offset);

            case TagDataType.String:
                // S7 String: Byte 0 = MaxLen, Byte 1 = ActualLen
                if (offset + 1 >= buffer.Length) return string.Empty;
                int actualLen = buffer[offset + 1];
                if (offset + 2 + actualLen > buffer.Length) return string.Empty;
                return Encoding.ASCII.GetString(buffer, offset + 2, actualLen);

            default:
                return null;
        }
    }

    private int GetTagByteLength(CommunicationTag tag)
    {
        if (tag.DataType == TagDataType.String) return Math.Max(1, (int)tag.ArrayLength);
        // For arrays of other types
        int count = Math.Max(1, (int)tag.ArrayLength);
        int typeSize = tag.DataType switch
        {
            TagDataType.Boolean => 1, // In bytes context
            TagDataType.Byte => 1,
            TagDataType.Int16 or TagDataType.UInt16 => 2,
            TagDataType.Int32 or TagDataType.UInt32 or TagDataType.Float => 4,
            TagDataType.Int64 or TagDataType.UInt64 or TagDataType.Double => 8,
            _ => 1
        };
        return count * typeSize;
    }

    private void ReadSingleTag(CommunicationTag tag, Dictionary<string, object?> dict)
    {
        if (!S7AddressParser.TryParse(tag.Address, out var addr))
        {
            dict[tag.Name] = null;
            return;
        }

        try
        {
            // 字符串：使用 ArrayLength 作为长度参数
            if (tag.DataType == TagDataType.String)
            {
                var len = Math.Max(1, (int)tag.ArrayLength);
                var r = _client!.ReadString($"DB{addr.Db}.{addr.Offset}", (ushort)len);
                dict[tag.Name] = r.IsSuccess ? r.Content : null;
                return;
            }

            // 数值类型数组：按类型大小顺序偏移
            int count = Math.Max(1, (int)tag.ArrayLength);
            if (count > 1 && addr.Kind != S7AddressKind.DBX)
            {
                dict[tag.Name] = ReadArray(_client!, tag.DataType, addr, count);
                return;
            }

            dict[tag.Name] = ReadScalar(_client!, tag.DataType, addr);
        }
        catch
        {
            dict[tag.Name] = null;
        }
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
