using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Models.DeviceCommunication;
using Plant01.Upper.Infrastructure.DeviceCommunication.Models;
using Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;
using Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;
using HslCommunication.ModBus;
using System.ComponentModel.DataAnnotations;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Drivers;

/// <summary>
/// Modbus TCP 驱动实现（演示扩展属性的使用）
/// </summary>
public class ModbusTcpDriver : IDriver
{
    private DeviceConfig? _config;
    private bool _isConnected;
    private ModbusTcpNet? _client;

    public bool IsConnected => _isConnected;

    public void Initialize(DeviceConfig config)
    {
        _config = config;
    }

    public void ValidateConfig(DeviceConfig config)
    {
        // 使用强类型配置和自动验证
        var driverConfig = config.GetAndValidateDriverConfig<ModbusTcpConfig>();
        
        // 验证已通过 DataAnnotations 自动完成
        // 标签的验证将在 ReadTagsAsync/WriteTagAsync 时进行
    }

    public Task ConnectAsync()
    {
        if (_config == null) throw new InvalidOperationException("驱动程序未初始化");

        var driverConfig = _config.GetDriverConfig<ModbusTcpConfig>();

        _client = new ModbusTcpNet(driverConfig.IpAddress, driverConfig.Port)
        {
            ConnectTimeOut = driverConfig.ConnectTimeout,
            ReceiveTimeOut = driverConfig.ReceiveTimeout
        };

        var res = _client.ConnectServer();
        _isConnected = res.IsSuccess;
        
        if (!_isConnected)
        {
            throw new InvalidOperationException($"Modbus TCP 连接失败: {res.Message}");
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
        if (_client == null || !_isConnected) 
            throw new InvalidOperationException("Modbus TCP 未连接");
            
        var result = new Dictionary<string, object?>();

        foreach (var tagObj in tags)
        {
            if (tagObj is not CommunicationTag tag) continue;

            try
            {
                // 使用扩展方法获取 Modbus 特定属性
                var stationId = tag.GetModbusStationId(defaultValue: 1);
                var functionCode = tag.GetModbusFunctionCode(defaultValue: 3);
                
                // 设置当前从站号
                _client.Station = stationId;
                
                // 根据数据类型读取
                object? value = tag.DataType switch
                {
                    TagDataType.Boolean => ReadBoolean(tag, functionCode),
                    TagDataType.Int16 => ReadInt16(tag, functionCode),
                    TagDataType.UInt16 => ReadUInt16(tag, functionCode),
                    TagDataType.Int32 => ReadInt32(tag, functionCode),
                    TagDataType.UInt32 => ReadUInt32(tag, functionCode),
                    TagDataType.Float => ReadFloat(tag, functionCode),
                    TagDataType.Double => ReadDouble(tag, functionCode),
                    TagDataType.String => ReadString(tag, functionCode),
                    _ => throw new NotSupportedException($"不支持的数据类型: {tag.DataType}")
                };

                result[tag.Name] = value;
            }
            catch (Exception ex)
            {
                result[tag.Name] = null;
                Console.WriteLine($"读取标签 {tag.Name} 失败: {ex.Message}");
            }
        }

        return Task.FromResult(result);
    }

    public Task WriteTagAsync(object tag, object value)
    {
        if (_client == null || !_isConnected) 
            throw new InvalidOperationException("Modbus TCP 未连接");

        if (tag is not CommunicationTag commTag) 
            throw new ArgumentException("标签类型不正确");

        // 使用扩展方法获取 Modbus 特定属性
        var stationId = commTag.GetModbusStationId(defaultValue: 1);
        _client.Station = stationId;

        var address = commTag.Address;

        var writeResult = commTag.DataType switch
        {
            TagDataType.Boolean => _client.Write(address, Convert.ToBoolean(value)),
            TagDataType.Int16 => _client.Write(address, Convert.ToInt16(value)),
            TagDataType.UInt16 => _client.Write(address, Convert.ToUInt16(value)),
            TagDataType.Int32 => _client.Write(address, Convert.ToInt32(value)),
            TagDataType.UInt32 => _client.Write(address, Convert.ToUInt32(value)),
            TagDataType.Float => _client.Write(address, Convert.ToSingle(value)),
            TagDataType.Double => _client.Write(address, Convert.ToDouble(value)),
            TagDataType.String => _client.Write(address, value.ToString() ?? ""),
            _ => throw new NotSupportedException($"不支持的数据类型: {commTag.DataType}")
        };

        if (!writeResult.IsSuccess)
        {
            throw new InvalidOperationException($"写入失败: {writeResult.Message}");
        }

        return Task.CompletedTask;
    }

    #region 私有读取方法

    private bool ReadBoolean(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            1 => _client!.ReadCoil(tag.Address),           // 读线圈
            2 => _client!.ReadDiscrete(tag.Address),       // 读离散输入
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 Boolean")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private short ReadInt16(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            3 => _client!.ReadInt16(tag.Address),          // 读保持寄存器
            4 => _client!.ReadInt16(tag.Address),          // 读输入寄存器
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 Int16")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private ushort ReadUInt16(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            3 => _client!.ReadUInt16(tag.Address),
            4 => _client!.ReadUInt16(tag.Address),
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 UInt16")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private int ReadInt32(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            3 => _client!.ReadInt32(tag.Address),
            4 => _client!.ReadInt32(tag.Address),
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 Int32")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private uint ReadUInt32(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            3 => _client!.ReadUInt32(tag.Address),
            4 => _client!.ReadUInt32(tag.Address),
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 UInt32")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private float ReadFloat(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            3 => _client!.ReadFloat(tag.Address),
            4 => _client!.ReadFloat(tag.Address),
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 Float")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private double ReadDouble(CommunicationTag tag, byte functionCode)
    {
        var result = functionCode switch
        {
            3 => _client!.ReadDouble(tag.Address),
            4 => _client!.ReadDouble(tag.Address),
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 Double")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private string ReadString(CommunicationTag tag, byte functionCode)
    {
        var length = Math.Max(1, (int)tag.ArrayLength);
        
        var result = functionCode switch
        {
            3 => _client!.ReadString(tag.Address, (ushort)length),
            4 => _client!.ReadString(tag.Address, (ushort)length),
            _ => throw new NotSupportedException($"功能码 {functionCode} 不支持读取 String")
        };
        
        return result.IsSuccess ? result.Content : throw new Exception(result.Message);
    }

    private static bool IsValidFunctionCode(byte code)
    {
        return code is 1 or 2 or 3 or 4 or 5 or 6 or 15 or 16;
    }

    #endregion
}
