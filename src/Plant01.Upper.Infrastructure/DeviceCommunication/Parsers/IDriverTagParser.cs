using CsvHelper;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.Parsers;

/// <summary>
/// 驱动特定标签属性解析器接口
/// </summary>
public interface IDriverTagParser
{
    /// <summary>
    /// 对应的驱动类型名称（如 SiemensS7Tcp, ModbusTcp）
    /// </summary>
    string DriverType { get; }

    /// <summary>
    /// 从 CSV 行解析扩展属性
    /// </summary>
    /// <param name="row">CSV 读取行上下文</param>
    /// <returns>扩展属性字典</returns>
    Dictionary<string, object> ParseExtendedProperties(IReaderRow row);
}
