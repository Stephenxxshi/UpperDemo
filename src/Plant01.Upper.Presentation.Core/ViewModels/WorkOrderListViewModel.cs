using AutoMapper;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using Plant01.Core.Data;
using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Application.Contracts.IntegrationEvents;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Presentation.Core.Services;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public class WorkOrderFilter
{
    public string? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public partial class WorkOrderListViewModel : EntityListViewModelBase<WorkOrderDto, WorkOrderFilter>
{
    private List<WorkOrderDto> _allProducts;
    private int _selectedCount;

    public int SelectedCount
    {
        get => _selectedCount;
        set => SetProperty(ref _selectedCount, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand BatchDeleteCommand { get; }

    private readonly ILogger<WorkOrderListViewModel> _logger;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IMapper _mapper;
    public ObservableCollection<WorkOrderDto> WorkOrders { get; set; } = new();

    public WorkOrderListViewModel(IDialogService dialogService, IMapper mapper, ILogger<WorkOrderListViewModel> logger, IWorkOrderRepository workOrderRepository) : base(dialogService)
    {
        _logger = logger;
        _workOrderRepository = workOrderRepository;
        _mapper = mapper;
        _allProducts = new List<WorkOrderDto>();

        // 异步加载数据，避免阻塞 UI 线程导致死锁或异常
        _ = LoadDataAsync();

        WeakReferenceMessenger.Default.Register<WorkOrderReceivedEvent>(this, (r, m) =>
        {
            _logger.LogTrace("Received WorkOrderReceivedEvent in WorkOrderListViewModel.");
            // 在这里处理收到的工单消息
            WorkOrders.Add(m.WorkOrder);
        });
    }

    private async Task LoadDataAsync()
    {
        try
        {
            //IsLoading = true;
            var all = await _workOrderRepository.GetAllAsync();
            _allProducts = _mapper.Map<List<WorkOrderDto>>(all);
            
            // 如果需要将数据绑定到界面，建议也更新 WorkOrders 集合
            // foreach (var item in _allProducts)
            // {
            //     WorkOrders.Add(item);
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load work orders.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected override Task<PagedResult<WorkOrderDto>> GetPagedDataAsync(PageRequest request)
    {
        throw new NotImplementedException();
    }
}
