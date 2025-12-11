using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.DTOs;

namespace Plant01.Upper.Application.Contracts.IntegrationEvents;

public record WorkOrderReceivedEvent(WorkOrderRequestDto WorkOrder);
