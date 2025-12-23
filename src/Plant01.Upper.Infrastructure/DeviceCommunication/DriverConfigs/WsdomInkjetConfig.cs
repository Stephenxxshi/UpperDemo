using System.ComponentModel.DataAnnotations;

namespace Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;

/// <summary>
/// Swdom 喷码机驱动配置
/// </summary>
public class WsdomInkjetConfig
{
    /// <summary>
    /// 喷码机 IP 地址
    /// </summary>
    [Required(ErrorMessage = "IpAddress 是必需的")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "IpAddress 格式无效")]
    public string IpAddress { get; set; } = "192.168.1.100";

    /// <summary>
    /// TCP 端口 (默认: 2000，请根据实际情况调整)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Port 必须在 1-65535 之间")]
    public int Port { get; set; } = 2000;

    /// <summary>
    /// 连接超时时间 (毫秒)
    /// </summary>
    public int ConnectTimeout { get; set; } = 3000;

    /// <summary>
    /// 接收超时时间 (毫秒)
    /// </summary>
    public int ReceiveTimeout { get; set; } = 3000;

    /// <summary>
    /// 状态查询最小间隔 (毫秒)
    /// </summary>
    public int StatusRefreshInterval { get; set; } = 500;

    /// <summary>
    /// 作业列表查询最小间隔 (毫秒)
    /// </summary>
    public int JobsRefreshInterval { get; set; } = 10000;

    /// <summary>
    /// 自定义命令刷新间隔 (命令 -> 毫秒)
    /// </summary>
    public Dictionary<string, int> CustomIntervals { get; set; } = new();
}
