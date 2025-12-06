using CommunityToolkit.Mvvm.ComponentModel;
using Plant01.Upper.Application.DTOs;
using Plant01.Upper.Application.Interfaces;
using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public partial class ProductionMonitorViewModel : ObservableObject
{
    private readonly IProductionQueryService _productionQueryService;

    [ObservableProperty]
    private ObservableCollection<BagDto> _bags = new();

    [ObservableProperty]
    private ObservableCollection<WorkOrderDto> _workOrders = new();

    [ObservableProperty]
    private ObservableCollection<PalletDto> _pallets = new();

    [ObservableProperty]
    private WorkOrderDto? _currentWorkOrder;

    public ProductionMonitorViewModel(IProductionQueryService productionQueryService)
    {
        _productionQueryService = productionQueryService;
    }

    public async Task LoadDataAsync()
    {
        // 加载最近的工单
        var orders = await _productionQueryService.GetRecentWorkOrdersAsync(10);
        WorkOrders = new ObservableCollection<WorkOrderDto>(orders);
        
        if (WorkOrders.Any())
        {
            CurrentWorkOrder = WorkOrders.First();
        }

        // 加载最近的袋子数据
        var bags = await _productionQueryService.GetRecentBagsAsync(50);
        Bags = new ObservableCollection<BagDto>(bags);

        // 加载托盘数据
        var pallets = await _productionQueryService.GetRecentPalletsAsync(10);
        Pallets = new ObservableCollection<PalletDto>(pallets);
    }
}
