using System.Text.Json;

namespace Plant01.Upper.Application.Models.DeviceCommunication;

public class DeviceConfig
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    // 特定于驱动的配置选项（IP、端口、机架、插槽、扫描速率等）
    public Dictionary<string, object> Options { get; set; } = new();

    /// <summary>
    /// 辅助方法：将通用的 Options 字典转换为特定驱动的强类型配置对象。
    /// </summary>
    /// <typeparam name="T">驱动特定的配置类类型</typeparam>
    /// <returns>强类型配置对象</returns>
    public T GetDriverConfig<T>() where T : new()
    {
        if (Options == null || Options.Count == 0) 
            return new T();

        try
        {
            // 利用 System.Text.Json 进行中转转换
            var json = JsonSerializer.Serialize(Options);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            }) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    // 用于存储设备的标签
    // 注意：标签可能独立存在，其逻辑可能位于此处。
    // 但是，根据架构原则将标签放置于独立平铺列表中，这样可能不适合此列表。
    // 根据当前模型，我们说它归属设备。
    // 然而，当前实施从 CSV 读取标签。
    // 目前，我们保持简单。
}
