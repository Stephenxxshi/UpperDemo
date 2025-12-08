using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Contracts.DTOs;

public class BagDto
{
    public required string BagCode { get; set; }
    public required string OrderCode { get; set; }
    public required string ProductCode { get; set; }
    public required string ProductAlias { get; set; }
    public float? ProductWeight { get; set; }
    public float? ProductActualWeight { get; set; }
    public required string BatchCode { get; set; }
    public bool IsNeedPrint { get; set; }
    public string? PalletCode { get; set; }
    public DateTime? PrintedAt { get; set; }
    public DateTime? PalletizedAt { get; set; }
    
    // 简化的状态描述，或者包含最新的 ProcessStep
    public required string CurrentStatus { get; set; }
    public List<BagProcessRecordDto> Records { get; set; } = new();
}

public class BagProcessRecordDto
{
    public ProcessStep Step { get; set; }
    public DateTime OccurredTime { get; set; }
    public required string MachineId { get; set; }
    public bool IsSuccess { get; set; }
    public required string Data { get; set; }
}
