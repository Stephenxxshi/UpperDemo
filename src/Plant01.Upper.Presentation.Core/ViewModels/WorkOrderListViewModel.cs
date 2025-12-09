using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Application.Contracts.IntegrationEvents;
using Plant01.Upper.Domain.Repository;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public partial class WorkOrderListViewModel : ObservableObject
{
    private readonly ILogger<WorkOrderListViewModel> _logger;
    private readonly IWorkOrderRepository _workOrderRepository;
    public ObservableCollection<WorkOrderDto> WorkOrders { get; set; } = new();

    public WorkOrderListViewModel(ILogger<WorkOrderListViewModel> logger, IWorkOrderRepository workOrderRepository)
    {
        _logger = logger;
        _workOrderRepository = workOrderRepository;

        WeakReferenceMessenger.Default.Register<WorkOrderReceivedEvent>(this, (r, m) =>
        {
            _logger.LogTrace("Received WorkOrderReceivedEvent in WorkOrderListViewModel.");
            // 在这里处理收到的工单消息
            WorkOrders.Add(m.WorkOrder);
        });
    }
}
