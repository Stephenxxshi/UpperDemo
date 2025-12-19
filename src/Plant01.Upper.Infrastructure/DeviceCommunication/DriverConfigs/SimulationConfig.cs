namespace Plant01.Upper.Infrastructure.DeviceCommunication.DriverConfigs;

/// <summary>
/// 仿真驱动配置
/// </summary>
public class SimulationConfig
{
    /// <summary>
    /// 仿真延迟 (毫秒, 默认: 50)
    /// </summary>
    public int SimulationDelay { get; set; } = 50;

    /// <summary>
    /// 随机数种子 (默认: 0 表示使用时间戳)
    /// </summary>
    public int RandomSeed { get; set; } = 0;
}
