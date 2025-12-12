using Plant01.Upper.Application.Models;

namespace Plant01.Upper.Application.Messages;

/// <summary>
/// 通用工站触发消息
/// </summary>
public record StationTriggerMessage(
    string TraceId,         // 全链路追踪 ID
    string StationId,       // 工站 ID
    TriggerSourceType Source,
    string Payload,         // 数据载荷 (如条码、重量、信号名)
    TriggerPriority Priority,
    DateTime Timestamp
);
