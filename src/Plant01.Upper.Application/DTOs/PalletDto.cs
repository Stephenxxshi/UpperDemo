namespace Plant01.Upper.Application.DTOs;

public class PalletDto
{
    public required string PalletCode { get; set; }
    public required string WorkOrderCode { get; set; }
    public bool IsFull { get; set; }
    public DateTime? OutTime { get; set; }
    public required string CurrentPalletizerId { get; set; }
    public int BagCount { get; set; }
}
