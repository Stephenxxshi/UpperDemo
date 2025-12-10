using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plant01.Core.Models.DynamicList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace wpfuidemo.ViewModels
{
    public class DemoEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }

    public partial class DynamicListDemoViewModel : ObservableObject
    {
        [ObservableProperty]
        private ListConfiguration _listConfig;

        [ObservableProperty]
        private Dictionary<string, object> _searchValues = new();

        [ObservableProperty]
        private ObservableCollection<DemoEntity> _entities = new();

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _pageIndex = 1;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private bool _isMockDataEnabled = true;

        public DynamicListDemoViewModel()
        {
            _listConfig = new ListConfiguration(); // 初始化以避免警告
            InitializeConfig();
            LoadData();
        }

        private void InitializeConfig()
        {
            ListConfig = new ListConfiguration
            {
                SearchFields = new List<SearchFieldConfig>
                {
                    new SearchFieldConfig { Key = "Keyword", Label = "名称/编码", Type = SearchControlType.Text },
                    new SearchFieldConfig { Key = "Status", Label = "状态", Type = SearchControlType.Select, Options = new[] { "Active", "Inactive", "Pending" } },
                    new SearchFieldConfig { Key = "StartDate", SecondaryKey = "EndDate", Label = "创建时间", Type = SearchControlType.DateRange }
                },
                Columns = new List<ColumnConfig>
                {
                    new ColumnConfig { Header = "编码", BindingPath = "Code", Width = 100, WidthType = "Pixel" },
                    new ColumnConfig { Header = "名称", BindingPath = "Name" },
                    new ColumnConfig { Header = "状态", BindingPath = "Status" },
                    new ColumnConfig { Header = "创建时间", BindingPath = "CreatedTime", StringFormat = "yyyy-MM-dd HH:mm:ss" }
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
            MessageBox.Show("Create Clicked");
        }

        [RelayCommand]
        private void Edit(DemoEntity entity)
        {
            MessageBox.Show($"Edit {entity.Name}");
        }

        [RelayCommand]
        private void Delete(DemoEntity entity)
        {
            MessageBox.Show($"Delete {entity.Name}");
        }

        [RelayCommand]
        private void ToggleMockData()
        {
            IsMockDataEnabled = !IsMockDataEnabled;
            LoadData();
        }

        private void LoadData()
        {
            if (!IsMockDataEnabled)
            {
                Entities.Clear();
                TotalCount = 0;
                return;
            }

            // 1. 生成模拟数据 (如果尚未生成或如果我们想要模拟数据库)
            var allData = GenerateMockData();

            // 2. 应用过滤器
            var filteredData = new List<DemoEntity>();
            foreach (var item in allData)
            {
                bool match = true;

                // 按关键字过滤 (名称或编码)
                if (SearchValues.ContainsKey("Keyword") && SearchValues["Keyword"] is string keyword && !string.IsNullOrWhiteSpace(keyword))
                {
                    if (!item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) && 
                        !item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                    }
                }

                // 按状态过滤
                if (match && SearchValues.ContainsKey("Status") && SearchValues["Status"] is string status && !string.IsNullOrWhiteSpace(status))
                {
                    if (!string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                    }
                }

                // 按日期范围过滤
                if (match)
                {
                    if (SearchValues.ContainsKey("StartDate") && SearchValues["StartDate"] is DateTime startDate)
                    {
                        if (item.CreatedTime.Date < startDate.Date) match = false;
                    }
                    if (match && SearchValues.ContainsKey("EndDate") && SearchValues["EndDate"] is DateTime endDate)
                    {
                        if (item.CreatedTime.Date > endDate.Date) match = false;
                    }
                }

                if (match)
                {
                    filteredData.Add(item);
                }
            }

            // 3. 应用分页
            TotalCount = filteredData.Count;
            var pagedData = new List<DemoEntity>();
            int skip = (PageIndex - 1) * PageSize;
            for (int i = skip; i < skip + PageSize && i < filteredData.Count; i++)
            {
                pagedData.Add(filteredData[i]);
            }

            Entities = new ObservableCollection<DemoEntity>(pagedData);
        }

        private List<DemoEntity> GenerateMockData()
        {
            var list = new List<DemoEntity>();
            var statuses = new[] { "Active", "Inactive", "Pending" };
            var random = new Random(42); // 固定种子以获得一致的结果

            for (int i = 1; i <= 100; i++)
            {
                list.Add(new DemoEntity
                {
                    Code = $"ORD-{i:0000}",
                    Name = $"Work Order {i} - " + (i % 3 == 0 ? "Maintenance" : i % 3 == 1 ? "Repair" : "Inspection"),
                    Status = statuses[i % 3],
                    CreatedTime = DateTime.Now.AddDays(-random.Next(0, 60)).AddHours(random.Next(0, 24))
                });
            }
            return list;
        }
    }
}
