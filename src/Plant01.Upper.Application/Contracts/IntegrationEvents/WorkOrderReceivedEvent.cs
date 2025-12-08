using Plant01.Upper.Application.Contracts.DTOs;

namespace Plant01.Upper.Application.Contracts.IntegrationEvents;

public record WorkOrderReceivedEvent(WorkOrderDto WorkOrder);
