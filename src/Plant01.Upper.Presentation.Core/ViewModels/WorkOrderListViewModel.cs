using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Application.Contracts.IntegrationEvents;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public partial class WorkOrderListViewModel : ObservableObject
{
    private readonly ILogger<WorkOrderListViewModel> _logger;
    public ObservableCollection<WorkOrderDto> WorkOrders { get; set; } = new();
    public WorkOrderListViewModel(ILogger<WorkOrderListViewModel> logger)
    {
        _logger = logger;

        WeakReferenceMessenger.Default.Register<WorkOrderReceivedEvent>(this, (r, m) =>
        {
            _logger.LogTrace("Received WorkOrderReceivedEvent in WorkOrderListViewModel.");
            // 在这里处理收到的工单消息
            WorkOrders.Add(m.WorkOrder);
        });
    }
}
