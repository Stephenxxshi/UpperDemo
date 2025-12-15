namespace Plant01.Upper.Domain.Models.DeviceCommunication;

public class DeviceConfig
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    // 特定于驱动程序的选项（IP、端口、插槽、机架、波特率等）
    public Dictionary<string, object> Options { get; set; } = new();

    // 属于此设备的标签
    // 注意：标签可能单独加载，但逻辑上属于此处。
    // 如果出于性能或其他原因将标签保存在单独的平面列表中，我们可能不会填充此列表，
    // 但对于配置模型来说是有意义的。
    // 然而，以前的实现从 CSV 加载标签。
    // 目前让我们保持简单。
}
