using System.ComponentModel.DataAnnotations;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;

/// <summary>
/// Modbus TCP 驱动配置
/// </summary>
public class ModbusTcpConfig
{
    /// <summary>
    /// Modbus TCP 服务器 IP 地址 (必需)
    /// </summary>
    [Required(ErrorMessage = "IpAddress 是必需的")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "IpAddress 格式无效")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// TCP 端口 (默认: 502)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Port 必须在 1-65535 之间")]
    public int Port { get; set; } = 502;

    /// <summary>
    /// 从站地址 (默认: 1)
    /// </summary>
    [Range(1, 247, ErrorMessage = "SlaveId 必须在 1-247 之间")]
    public int SlaveId { get; set; } = 1;

    /// <summary>
    /// 扫描速率 (毫秒, 默认: 100)
    /// </summary>
    [Range(10, 10000, ErrorMessage = "ScanRate 必须在 10-10000 毫秒之间")]
    public int ScanRate { get; set; } = 100;

    /// <summary>
    /// 连接超时 (毫秒, 默认: 5000)
    /// </summary>
    [Range(1000, 60000, ErrorMessage = "ConnectTimeout 必须在 1000-60000 毫秒之间")]
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// 接收超时 (毫秒, 默认: 5000)
    /// </summary>
    [Range(1000, 60000, ErrorMessage = "ReceiveTimeout 必须在 1000-60000 毫秒之间")]
    public int ReceiveTimeout { get; set; } = 5000;
}
