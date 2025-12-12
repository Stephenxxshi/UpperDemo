using Plant01.Domain.Shared.Events;

namespace Plant01.Upper.Domain.Events;

/// <summary>
/// 托盘满垛事件
/// </summary>
public class PalletFullEvent : DomainEventBase
{
    public string PalletCode { get; }
    public string WorkOrderCode { get; }
    public int BagCount { get; }

    public PalletFullEvent(string palletCode, string workOrderCode, int bagCount)
    {
        PalletCode = palletCode;
        WorkOrderCode = workOrderCode;
        BagCount = bagCount;
    }
}

/// <summary>
/// 托盘出垛事件 (完成)
/// </summary>
public class PalletDischargedEvent : DomainEventBase
{
    public string PalletCode { get; }
    public string WorkOrderCode { get; }
    public DateTime DischargedAt { get; }

    public PalletDischargedEvent(string palletCode, string workOrderCode, DateTime dischargedAt)
    {
        PalletCode = palletCode;
        WorkOrderCode = workOrderCode;
        DischargedAt = dischargedAt;
    }
}
