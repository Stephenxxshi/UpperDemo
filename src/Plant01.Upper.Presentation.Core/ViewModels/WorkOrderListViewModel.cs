using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using Plant01.Core.Models.DynamicList;
using Plant01.Upper.Application.Contracts.Api.Requests;
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
    private ObservableCollection<WorkOrderRequestDto> _workOrders = new();

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
                new ColumnConfig { Header = "工单号", BindingPath = "Code", Width = 1 ,WidthType = ColumnWidthType.Star},
                    new ColumnConfig { Header = "产线编号", BindingPath = "LineNo", Width = 100 },
                    new ColumnConfig { Header = "产品编号", BindingPath = "ProductCode", Width = 120 },
                    new ColumnConfig { Header = "产品名称", BindingPath = "ProductName", Width = 120 },
                    new ColumnConfig { Header = "产品规格", BindingPath = "ProductSpec", Width = 120 },
                    new ColumnConfig { Header = "计划数量", BindingPath = "Quantity", Width = 120 },
                    new ColumnConfig { Header = "单位", BindingPath = "Unit", Width = 100 },
                    new ColumnConfig { Header = "批号", BindingPath = "BatchNumber", Width = 100 },
                    new ColumnConfig { Header = "工单状态", BindingPath = "Status", Width=120 },
                    new ColumnConfig { Header = "创建日期", BindingPath = "OrderDate", Width=150, StringFormat = "yyyy/MM/dd" },
                    new ColumnConfig { Header = "产品编号", BindingPath = "ProductCode", Width=120 }

            },
            RowActions = new List<RowActionConfig>
                {
                    new RowActionConfig { Label = "编辑",ToolTip="编辑", Command = EditCommand, Icon = "&#xe737;", DisplayMode= RowActionDisplayMode.Icon },
                    new RowActionConfig { Label = "删除",ToolTip="删除", Command = DeleteCommand,Icon = "&#xe612;", DisplayMode= RowActionDisplayMode.Icon, ColorTokenType = DesignTokenType.ErrorColor }
                }
        };

        // 初始化搜索条件字典的键，确保绑定能够正常工作
        foreach (var field in ListConfig.SearchFields)
        {
            if (!SearchValues.ContainsKey(field.Key))
            {
                SearchValues[field.Key] = null;
            }
        }
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

        foreach (var field in ListConfig.SearchFields)
        {
            SearchValues[field.Key] = null;
        }

        OnPropertyChanged(nameof(SearchValues));
        LoadData();
    }

    [RelayCommand]
    private void Create()
    {
        _logger.LogInformation("点击创建按钮。");
        _dialogService.ShowDialog(this, null, (result) =>
        {
            if (result is WorkOrderRequestDto newEntity)
            {
                // 处理新创建的实体
                _logger.LogInformation("工单创建成功: {Code}", newEntity.Code);
            }
        });
    }

    [RelayCommand]
    private void Edit(WorkOrderRequestDto entity)
    {
        _logger.LogInformation("点击编辑按钮。工单: {Code}", entity.Code);
        _dialogService.ShowDialog(this, entity, (result) =>
        {
            if (result is WorkOrderRequestDto editedEntity)
            {
                // 处理编辑后的实体
                _logger.LogInformation("工单编辑成功: {Code}", editedEntity.Code);
            }
        });
    }

    [RelayCommand]
    private void Delete(WorkOrderRequestDto entity)
    {
        _logger.LogInformation("点击删除按钮。工单: {Code}", entity.Code);
        _dialogService.ShowDialog(this, entity, (result) =>
        {
            if (result is WorkOrderRequestDto deletedEntity)
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

            //if (allData.Count == 0)
            //{
            //    _logger.LogInformation("数据库为空，生成模拟数据...");
            //    allData = GenerateMockData();
            //    foreach (var item in allData)
            //    {
            //        await _workOrderRepository.AddAsync(item);
            //    }
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载工单时出错。");
            throw;
        }
        // 2. 应用过滤器
        var filteredData = new List<WorkOrderRequestDto>();
        foreach (var item in allData)
        {
            bool match = true;

            // 按关键字过滤 (名称或编码)
            if (SearchValues.ContainsKey("Code") && SearchValues["Code"] is string code && !string.IsNullOrWhiteSpace(code))
            {
                if (!item.Code.Contains(code, StringComparison.OrdinalIgnoreCase) &&
                    !item.ProductName.Contains(code, StringComparison.OrdinalIgnoreCase))
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
                filteredData.Add(_mapper.Map<WorkOrderRequestDto>(item));
            }
        }

        // 3. 应用分页
        TotalCount = filteredData.Count;

        if (PageIndex < 1) PageIndex = 1;

        var pagedData = filteredData
            .Skip((PageIndex - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        WorkOrders = new ObservableCollection<WorkOrderRequestDto>(pagedData);
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

    partial void OnPageIndexChanged(int value)
    {
        LoadData();
    }

    partial void OnPageSizeChanged(int value)
    {
        // 切换页大小时，通常重置到第一页
        if (PageIndex != 1)
        {
            PageIndex = 1;
        }
        else
        {
            LoadData();
        }
    }
}
