using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using Plant01.Core.Models.DynamicList;
using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;
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

        _listConfig = new ListConfiguration(); // 初始化以避免警告
        InitializeConfig();
        LoadData();
    }

    private void InitializeConfig()
    {
        _logger.LogInformation("初始化工单列表配置。");
        ListConfig = new ListConfiguration
        {
            SearchFields = new List<SearchFieldConfig>
                {
                    new SearchFieldConfig { Key = "Code", Label = "工单号", Type = SearchControlType.Text },
                    new SearchFieldConfig { Key = "Status", Label = "工单状态", Type = SearchControlType.Select, Options = new[] { WorkOrderStatus.开工.ToString(), WorkOrderStatus.完工.ToString() } },
                    new SearchFieldConfig { Key = "StartDate", SecondaryKey = "EndDate", Label = "创建时间", Type = SearchControlType.DateRange }
                },
            Columns = new List<ColumnConfig>
                {
                new ColumnConfig { Header = "工单号", BindingPath = "Code", Width = 200 },
                    new ColumnConfig { Header = "产线编号", BindingPath = "LineNo", Width = 100 },
                    new ColumnConfig { Header = "产品编号", BindingPath = "ProductCode", Width = 100 },
                    new ColumnConfig { Header = "产品名称", BindingPath = "ProductName", Width = 100 },
                    new ColumnConfig { Header = "产品规格", BindingPath = "ProductSpec", Width = 100 },
                    new ColumnConfig { Header = "计划数量", BindingPath = "Quantity", Width = 100 },
                    new ColumnConfig { Header = "单位", BindingPath = "Unit", Width = 100 },
                    new ColumnConfig { Header = "批号", BindingPath = "BatchNumber", Width = 100 },
                    new ColumnConfig { Header = "工单状态", BindingPath = "Status", Width=100 },
                    new ColumnConfig { Header = "创建日期", BindingPath = "OrderDate", Width=150, StringFormat = "yyyy-MM-dd HH:mm:ss" },
                    new ColumnConfig { Header = "产品编号", BindingPath = "ProductCode", Width=150 }

            },
            RowActions = new List<RowActionConfig>
                {
                    new RowActionConfig { Label = "编辑",ToolTip="编辑", Command = EditCommand, Icon = "&#xe737;", DisplayMode= RowActionDisplayMode.Icon },
                    new RowActionConfig { Label = "删除",ToolTip="删除", Command = DeleteCommand,Icon = "&#xe612;", DisplayMode= RowActionDisplayMode.Icon    }
                }
        };
    }

    [RelayCommand]
    private void Search()
    {
        _logger.LogInformation("执行搜索。");
        LoadData();
    }

    [RelayCommand]
    private void Reset()
    {
        _logger.LogInformation("重置搜索条件。");
        SearchValues.Clear();
        SearchValues = new Dictionary<string, object>();
        OnPropertyChanged(nameof(SearchValues));
        LoadData();
    }

    [RelayCommand]
    private void Create()
    {
        _logger.LogInformation("点击创建按钮。");
        _dialogService.ShowDialog(this, null, (result) =>
        {
            if (result is WorkOrderDto newEntity)
            {
                // 处理新创建的实体
                _logger.LogInformation("工单创建成功: {Code}", newEntity.Code);
            }
        });
    }

    [RelayCommand]
    private void Edit(WorkOrderDto entity)
    {
        _logger.LogInformation("点击编辑按钮。工单: {Code}", entity.Code);
        _dialogService.ShowDialog(this, entity, (result) =>
        {
            if (result is WorkOrderDto editedEntity)
            {
                // 处理编辑后的实体
                _logger.LogInformation("工单编辑成功: {Code}", editedEntity.Code);
            }
        });
    }

    [RelayCommand]
    private void Delete(WorkOrderDto entity)
    {
        _logger.LogInformation("点击删除按钮。工单: {Code}", entity.Code);
        _dialogService.ShowDialog(this, entity, (result) =>
        {
            if (result is WorkOrderDto deletedEntity)
            {
                // 处理删除后的实体
                _logger.LogInformation("工单删除成功: {Code}", deletedEntity.Code);
            }
        });
    }

    private async void LoadData()
    {
        _logger.LogInformation("开始加载数据。页码: {PageIndex}, 页大小: {PageSize}", PageIndex, PageSize);
        List<WorkOrder> allData = new();
        try
        {
            // 1. 生成模拟数据 (如果尚未生成或如果我们想要模拟数据库)
            allData = await _workOrderRepository.GetAllAsync();

            if (allData.Count == 0)
            {
                _logger.LogInformation("数据库为空，生成模拟数据...");
                allData = GenerateMockData();
                foreach (var item in allData)
                {
                    await _workOrderRepository.AddAsync(item);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载工单时出错。");
            throw;
        }
        // 2. 应用过滤器
        var filteredData = new List<WorkOrderDto>();
        foreach (var item in allData)
        {
            bool match = true;

            // 按关键字过滤 (名称或编码)
            if (SearchValues.ContainsKey("Keyword") && SearchValues["Keyword"] is string keyword && !string.IsNullOrWhiteSpace(keyword))
            {
                if (!item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    !item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                }
            }

            // 按状态过滤
            if (match && SearchValues.ContainsKey("Status") && SearchValues["Status"] is string status && !string.IsNullOrWhiteSpace(status))
            {
                if (!string.Equals(item.Status.ToString(), status, StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                }
            }

            // 按日期范围过滤
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

        // 3. 应用分页
        TotalCount = filteredData.Count;
        var pagedData = new List<WorkOrderDto>();
        int skip = (PageIndex - 1) * PageSize;
        for (int i = skip; i < skip + PageSize && i < filteredData.Count; i++)
        {
            pagedData.Add(filteredData[i]);
        }

        WorkOrders = new ObservableCollection<WorkOrderDto>(pagedData);
        _logger.LogInformation("数据加载完成。总记录数: {TotalCount}", TotalCount);
    }

    private List<WorkOrder> GenerateMockData()
    {
        var list = new List<WorkOrder>();
        var random = new Random();
        for (int i = 1; i <= 20; i++)
        {
            list.Add(new WorkOrder
            {
                Code = $"WO-{DateTime.Now:yyyyMMdd}-{i:000}",
                Status = i % 2 == 0 ? WorkOrderStatus.开工 : WorkOrderStatus.完工,
                OrderDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(0, 30))),
                ProductName = $"Product {i}",
                ProductCode = $"P{i:000}",
                Quantity = random.Next(10, 1000),
                Unit = "PCS",
                LineNo = "L01",
                BatchNumber = $"B{DateTime.Now:yyyyMMdd}{i:00}",
                ProductSpec = "Standard",
                LabelTemplateCode = "T01"
            });
        }
        return list;
    }
}
