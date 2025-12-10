using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;
using Plant01.Upper.Presentation.Core.Models.DynamicList;
using Plant01.Upper.Presentation.Core.Services;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;


public partial class WorkOrderListViewModel : ObservableObject
{
    private readonly ILogger<WorkOrderListViewModel> _logger;
    private readonly IDialogService _dialogService;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IMapper _mapper;

    [ObservableProperty]
    private ListConfiguration _listConfig;

    [ObservableProperty]
    private Dictionary<string, object> _searchValues = new();

    [ObservableProperty]
    private ObservableCollection<WorkOrderDto> _workOrders = new();

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _pageIndex = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public WorkOrderListViewModel(ILogger<WorkOrderListViewModel> logger, IDialogService dialogService, IWorkOrderRepository workOrderRepository, IMapper mapper)
    {
        _logger = logger;
        _dialogService = dialogService;
        _workOrderRepository = workOrderRepository;
        _mapper = mapper;

        _listConfig = new ListConfiguration(); // Initialize to avoid warning
        InitializeConfig();
        LoadData();
    }

    private void InitializeConfig()
    {
        ListConfig = new ListConfiguration
        {
            SearchFields = new List<SearchFieldConfig>
                {
                    new SearchFieldConfig { Key = "Code", Label = "订单号", Type = SearchControlType.Text },
                    new SearchFieldConfig { Key = "Status", Label = "订单状态", Type = SearchControlType.Select, Options = new[] { WorkOrderStatus.开工.ToString(), WorkOrderStatus.完工.ToString() } },
                    new SearchFieldConfig { Key = "StartDate", SecondaryKey = "EndDate", Label = "创建时间", Type = SearchControlType.DateRange }
                },
            Columns = new List<ColumnConfig>
                {
                    new ColumnConfig { Header = "订单号", BindingPath = "Code", Width = 100, WidthType = "Pixel" },
                    new ColumnConfig { Header = "状态", BindingPath = "Status" },
                    new ColumnConfig { Header = "创建时间", BindingPath = "OrderDate", StringFormat = "yyyy-MM-dd HH:mm:ss" }
                },
            RowActions = new List<RowActionConfig>
                {
                    new RowActionConfig { Label = "编辑", Command = EditCommand },
                    new RowActionConfig { Label = "删除", Command = DeleteCommand }
                }
        };
    }

    [RelayCommand]
    private void Search()
    {
        LoadData();
    }

    [RelayCommand]
    private void Reset()
    {
        SearchValues.Clear();
        SearchValues = new Dictionary<string, object>();
        OnPropertyChanged(nameof(SearchValues));
        LoadData();
    }

    [RelayCommand]
    private void Create()
    {
        _dialogService.ShowDialog(this, null, (result) =>
        {
            if (result is WorkOrderDto newEntity)
            {
                // Handle the newly created entity
            }
        });
    }

    [RelayCommand]
    private void Edit(WorkOrderDto entity)
    {
        _dialogService.ShowDialog(this, entity, (result) =>
        {
            if (result is WorkOrderDto editedEntity)
            {
                // Handle the edited entity
            }
        });
    }

    [RelayCommand]
    private void Delete(WorkOrderDto entity)
    {
        _dialogService.ShowDialog(this, entity, (result) =>
        {
            if (result is WorkOrderDto deletedEntity)
            {
                // Handle the deleted entity
            }
        });
    }

    private async void LoadData()
    {
        List<WorkOrder> allData = new();
        try
        {
            // 1. Generate Mock Data (if not already generated or if we want to simulate DB)
            allData = await _workOrderRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading work orders.");
            throw;
        }
        // 2. Apply Filters
        var filteredData = new List<WorkOrderDto>();
        foreach (var item in allData)
        {
            bool match = true;

            // Filter by Keyword (Name or Code)
            if (SearchValues.ContainsKey("Keyword") && SearchValues["Keyword"] is string keyword && !string.IsNullOrWhiteSpace(keyword))
            {
                if (!item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    !item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                }
            }

            // Filter by Status
            if (match && SearchValues.ContainsKey("Status") && SearchValues["Status"] is string status && !string.IsNullOrWhiteSpace(status))
            {
                if (!string.Equals(item.Status.ToString(), status, StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                }
            }

            // Filter by Date Range
            if (match)
            {
                if (SearchValues.ContainsKey("StartDate") && SearchValues["StartDate"] is DateTime startDate)
                {
                    if (item.OrderDate < DateOnly.FromDateTime(startDate)) match = false;
                }
                if (match && SearchValues.ContainsKey("EndDate") && SearchValues["EndDate"] is DateTime endDate)
                {
                    if (item.OrderDate > DateOnly.FromDateTime(endDate)) match = false;
                }
            }

            if (match)
            {
                filteredData.Add(_mapper.Map<WorkOrderDto>(item));
            }
        }

        // 3. Apply Pagination
        TotalCount = filteredData.Count;
        var pagedData = new List<WorkOrderDto>();
        int skip = (PageIndex - 1) * PageSize;
        for (int i = skip; i < skip + PageSize && i < filteredData.Count; i++)
        {
            pagedData.Add(filteredData[i]);
        }

        WorkOrders = new ObservableCollection<WorkOrderDto>(pagedData);
    }


}
