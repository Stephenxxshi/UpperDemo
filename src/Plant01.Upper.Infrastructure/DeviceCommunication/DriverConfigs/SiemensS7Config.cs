using System.ComponentModel.DataAnnotations;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;

/// <summary>
/// 西门子 S7 PLC 驱动配置
/// </summary>
public class SiemensS7Config
{
    /// <summary>
    /// PLC IP 地址 (必需)
    /// </summary>
    [Required(ErrorMessage = "IpAddress 是必需的")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "IpAddress 格式无效")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// TCP 端口 (默认: 102)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Port 必须在 1-65535 之间")]
    public int Port { get; set; } = 102;

    /// <summary>
    /// 机架号 (默认: 0)
    /// </summary>
    [Range(0, 7, ErrorMessage = "Rack 必须在 0-7 之间")]
    public int Rack { get; set; } = 0;

    /// <summary>
    /// 插槽号 (默认: 1)
    /// </summary>
    [Range(0, 31, ErrorMessage = "Slot 必须在 0-31 之间")]
    public int Slot { get; set; } = 1;

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
    /// PLC 型号 (S7-200, S7-300, S7-400, S7-1200, S7-1500)
    /// </summary>
    public string PlcModel { get; set; } = "S7_1200";
}
