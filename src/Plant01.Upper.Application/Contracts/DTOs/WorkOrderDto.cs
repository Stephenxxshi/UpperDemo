using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Contracts.DTOs;

public class WorkOrderDto
{
    public required string Code { get; set; }
    public DateOnly OrderDate { get; set; }
    public required string LineNo { get; set; }
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public required string ProductSpec { get; set; }
    public int Quantity { get; set; }
    public required string Unit { get; set; }
    public required string BatchNumber { get; set; }
    public WorkOrderStatus Status { get; set; }
}
