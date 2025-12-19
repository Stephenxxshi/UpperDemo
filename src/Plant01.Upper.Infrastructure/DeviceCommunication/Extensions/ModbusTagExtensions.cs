using Plant01.Upper.Infrastructure.DeviceCommunication.Models;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Extensions;

/// <summary>
/// Modbus 驱动的标签扩展方法
/// </summary>
public static class ModbusTagExtensions
{
    /// <summary>
    /// 获取 Modbus 从站地址
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 1）</param>
    /// <returns>从站地址</returns>
    public static byte GetModbusStationId(this CommunicationTag tag, byte defaultValue = 1)
    {
        return tag.GetExtendedProperty("ModbusStationId", defaultValue);
    }

    /// <summary>
    /// 设置 Modbus 从站地址
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="stationId">从站地址（1-247）</param>
    public static void SetModbusStationId(this CommunicationTag tag, byte stationId)
    {
        if (stationId < 1 || stationId > 247)
        {
            throw new ArgumentOutOfRangeException(nameof(stationId), "从站地址必须在 1-247 范围内");
        }
        tag.SetExtendedProperty("ModbusStationId", stationId);
    }

    /// <summary>
    /// 获取 Modbus 功能码
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值（默认为 3，读保持寄存器）</param>
    /// <returns>功能码</returns>
    public static byte GetModbusFunctionCode(this CommunicationTag tag, byte defaultValue = 3)
    {
        return tag.GetExtendedProperty("ModbusFunctionCode", defaultValue);
    }

    /// <summary>
    /// 设置 Modbus 功能码
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="functionCode">功能码（1/2/3/4/5/6/15/16）</param>
    public static void SetModbusFunctionCode(this CommunicationTag tag, byte functionCode)
    {
        if (!IsValidModbusFunctionCode(functionCode))
        {
            throw new ArgumentException($"不支持的 Modbus 功能码: {functionCode}", nameof(functionCode));
        }
        tag.SetExtendedProperty("ModbusFunctionCode", functionCode);
    }

    /// <summary>
    /// 获取 Modbus 寄存器区域类型（可选，用于更明确的配置）
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>寄存器区域类型（Coil/DiscreteInput/HoldingRegister/InputRegister）</returns>
    public static string GetModbusRegisterType(this CommunicationTag tag, string defaultValue = "HoldingRegister")
    {
        return tag.GetExtendedProperty("ModbusRegisterType", defaultValue);
    }

    /// <summary>
    /// 设置 Modbus 寄存器区域类型
    /// </summary>
    /// <param name="tag">标签对象</param>
    /// <param name="registerType">寄存器区域类型</param>
    public static void SetModbusRegisterType(this CommunicationTag tag, string registerType)
    {
        tag.SetExtendedProperty("ModbusRegisterType", registerType);
    }

    /// <summary>
    /// 验证 Modbus 功能码是否合法
    /// </summary>
    private static bool IsValidModbusFunctionCode(byte code)
    {
        return code is 1 or 2 or 3 or 4 or 5 or 6 or 15 or 16;
    }
}
