namespace Plant01.Upper.Application.Messages;

/// <summary>
/// 标签值变化消息（用于状态监控、UI更新等非流程触发场景）
/// </summary>
public record TagValueChangedMessage(
    string EquipmentCode,   // 设备编号
    string TagCode,        // 标签代码
    //string TagName,         // 标签名称
    object? NewValue,       // 新值
    object? OldValue,       // 旧值
    string Purpose,         // 标签用途
    DateTime Timestamp      // 时间戳
);
