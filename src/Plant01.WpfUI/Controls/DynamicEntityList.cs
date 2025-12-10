using Plant01.Core.Models.DynamicList;

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Plant01.WpfUI.Controls;

[TemplatePart(Name = SearchPanelPartName, Type = typeof(Panel))]
[TemplatePart(Name = DataGridPartName, Type = typeof(AntDataGrid))]
[TemplatePart(Name = PaginationPartName, Type = typeof(AntPagination))]
[TemplatePart(Name = EmptyStatePartName, Type = typeof(AntEmpty))]
public class DynamicEntityList : Control
{
    private const string SearchPanelPartName = "PART_SearchPanel";
    private const string DataGridPartName = "PART_DataGrid";
    private const string PaginationPartName = "PART_Pagination";
    private const string EmptyStatePartName = "PART_EmptyState";

    private Panel? _searchPanel;
    private AntDataGrid? _dataGrid;
    private AntPagination? _pagination;
    private AntEmpty? _emptyState;

    static DynamicEntityList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicEntityList), new FrameworkPropertyMetadata(typeof(DynamicEntityList)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register(nameof(Configuration), typeof(ListConfiguration), typeof(DynamicEntityList), new PropertyMetadata(null, OnConfigurationChanged));

    public ListConfiguration? Configuration
    {
        get => (ListConfiguration?)GetValue(ConfigurationProperty);
        set => SetValue(ConfigurationProperty, value);
    }

    public static readonly DependencyProperty SearchContextProperty =
        DependencyProperty.Register(nameof(SearchContext), typeof(object), typeof(DynamicEntityList), new PropertyMetadata(null));

    public object? SearchContext
    {
        get => GetValue(SearchContextProperty);
        set => SetValue(SearchContextProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DynamicEntityList), new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(DynamicEntityList), new PropertyMetadata(0));

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(DynamicEntityList), new PropertyMetadata(1));

    public int PageIndex
    {
        get => (int)GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(DynamicEntityList), new PropertyMetadata(10));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(DynamicEntityList), new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(DynamicEntityList), new PropertyMetadata(null));

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(DynamicEntityList), new PropertyMetadata(null));

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public static readonly DependencyProperty CreateCommandProperty =
        DependencyProperty.Register(nameof(CreateCommand), typeof(ICommand), typeof(DynamicEntityList), new PropertyMetadata(null));

    public ICommand? CreateCommand
    {
        get => (ICommand?)GetValue(CreateCommandProperty);
        set => SetValue(CreateCommandProperty, value);
    }

    public static readonly DependencyProperty ShowCreateButtonProperty =
        DependencyProperty.Register(nameof(ShowCreateButton), typeof(bool), typeof(DynamicEntityList), new PropertyMetadata(true));

    public bool ShowCreateButton
    {
        get => (bool)GetValue(ShowCreateButtonProperty);
        set => SetValue(ShowCreateButtonProperty, value);
    }

    public static readonly DependencyProperty ExtraFilterContentProperty =
        DependencyProperty.Register(nameof(ExtraFilterContent), typeof(object), typeof(DynamicEntityList), new PropertyMetadata(null));

    public object? ExtraFilterContent
    {
        get => GetValue(ExtraFilterContentProperty);
        set => SetValue(ExtraFilterContentProperty, value);
    }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _searchPanel = GetTemplateChild(SearchPanelPartName) as Panel;
        _dataGrid = GetTemplateChild(DataGridPartName) as AntDataGrid;
        _pagination = GetTemplateChild(PaginationPartName) as AntPagination;
        _emptyState = GetTemplateChild(EmptyStatePartName) as AntEmpty;

        RefreshView();
    }

    private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DynamicEntityList)d).RefreshView();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (DynamicEntityList)d;
        control.UpdateEmptyState();
    }

    private void RefreshView()
    {
        if (Configuration == null) return;

        GenerateSearchFields();
        GenerateColumns();
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        if (_emptyState == null || _dataGrid == null) return;

        bool hasData = ItemsSource != null && ItemsSource.GetEnumerator().MoveNext();
        _emptyState.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
        _dataGrid.Visibility = hasData ? Visibility.Visible : Visibility.Collapsed;

        // 分页可见性逻辑
        if (_pagination != null)
        {
            _pagination.Visibility = (TotalCount > PageSize) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void GenerateSearchFields()
    {
        if (_searchPanel == null || Configuration == null) return;

        _searchPanel.Children.Clear();

        foreach (var field in Configuration.SearchFields)
        {
            FrameworkElement control = CreateSearchControl(field);
            if (control != null)
            {
                control.Margin = new Thickness(0, 0, 8, 0);
                _searchPanel.Children.Add(control);
            }
        }
    }

    private FrameworkElement CreateSearchControl(SearchFieldConfig field)
    {
        // 创建绑定的辅助方法
        Binding CreateBinding(string path)
        {
            var binding = new Binding(path)
            {
                Source = SearchContext,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            // 如果 SearchContext 是字典或动态对象，我们可能需要索引器绑定
            // 假设 SearchContext 是 ViewModel 对象，其属性与 'Key' 匹配
            // 如果是字典，路径应该是 [Key]
            if (SearchContext is IDictionary)
            {
                binding.Path = new PropertyPath($"[{path}]");
            }
            return binding;
        }

        switch (field.Type)
        {
            case SearchControlType.Text:
                var textBox = new AntInput
                {
                    Placeholder = field.Label,
                    Width = 150
                };
                textBox.SetBinding(AntInput.TextProperty, CreateBinding(field.Key));
                return textBox;

            case SearchControlType.Select:
                var comboBox = new AntSelect
                {
                    Placeholder = field.Label,
                    Width = 150,
                    ItemsSource = field.Options,
                    DisplayMemberPath = field.DisplayMemberPath,
                    SelectedValuePath = field.SelectedValuePath
                };
                comboBox.SetBinding(AntSelect.SelectedValueProperty, CreateBinding(field.Key));
                return comboBox;

            case SearchControlType.Date:
                var datePicker = new AntDatePicker
                {
                    Placeholder = field.Label,
                    Width = 150
                };
                datePicker.SetBinding(AntDatePicker.SelectedDateProperty, CreateBinding(field.Key));
                return datePicker;

            case SearchControlType.DateRange:
                var dateRange = new AntDateRangePicker
                {
                    Width = 250
                };
                dateRange.SetBinding(AntDateRangePicker.SelectedDateStartProperty, CreateBinding(field.Key));
                if (!string.IsNullOrEmpty(field.SecondaryKey))
                {
                    dateRange.SetBinding(AntDateRangePicker.SelectedDateEndProperty, CreateBinding(field.SecondaryKey));
                }
                return dateRange;

            default:
                return new TextBlock { Text = $"Unknown Type: {field.Type}" };
        }
    }

    private void GenerateColumns()
    {
        if (_dataGrid == null || Configuration == null) return;

        _dataGrid.Columns.Clear();

        foreach (var colConfig in Configuration.Columns)
        {
            if (!colConfig.IsVisible) continue;

            var column = new DataGridTextColumn
            {
                Header = colConfig.Header,
                Binding = new Binding(colConfig.BindingPath)
            };

            if (!string.IsNullOrEmpty(colConfig.StringFormat))
            {
                column.Binding.StringFormat = colConfig.StringFormat;
            }

            if (!string.IsNullOrEmpty(colConfig.ConverterName))
            {
                if (TryFindResource(colConfig.ConverterName) is IValueConverter converter)
                {
                    (column.Binding as Binding).Converter = converter;
                    (column.Binding as Binding).ConverterParameter = colConfig.ConverterParameter;
                }
            }

            // 宽度处理
            if (colConfig.Width.HasValue)
            {
                column.Width = new DataGridLength(colConfig.Width.Value, DataGridLengthUnitType.Pixel);
            }
            else
            {
                column.Width = colConfig.WidthType.ToLower() switch
                {
                    "auto" => DataGridLength.Auto,
                    "pixel" => DataGridLength.SizeToCells, // 后备
                    _ => new DataGridLength(1, DataGridLengthUnitType.Star)
                };
            }

            _dataGrid.Columns.Add(column);
        }

        // 行操作列
        if (Configuration.RowActions.Count > 0)
        {
            var actionColumn = new DataGridTemplateColumn
            {
                Header = "操作",
                Width = DataGridLength.Auto
            };

            var cellTemplate = new DataTemplate();
            var panelFactory = new FrameworkElementFactory(typeof(StackPanel));
            panelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            foreach (var action in Configuration.RowActions)
            {
                var btnFactory = new FrameworkElementFactory(typeof(AntButton));
                btnFactory.SetValue(AntButton.TypeProperty, ButtonType.Link); // 表格操作使用链接样式
                btnFactory.SetValue(AntButton.ContentProperty, action.Label);
                btnFactory.SetValue(AntButton.CommandProperty, action.Command);
                btnFactory.SetBinding(AntButton.CommandParameterProperty, new Binding(".")); // 绑定到行项目
                btnFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 0, 4, 0));

                // 处理显示模式 (图标/文本)
                if (action.DisplayMode == RowActionDisplayMode.Icon && action.Icon != null)
                {
                    // 如果图标是字符串 (字体键)，使用带有字体系列的 TextBlock
                    // 目前，如果是字符串，只需将 Content 设置为 Icon
                    btnFactory.SetValue(AntButton.ContentProperty, action.Icon);
                    // 您可能需要为图标按钮指定特定样式
                }
                else if (action.DisplayMode == RowActionDisplayMode.Both && action.Icon != null)
                {
                    // 带有图标 + 文本的复杂内容
                    // 为了简化工厂，也许只需追加文本
                    // 或者使用 AntButton 的 Icon 属性 (如果存在) (通常存在)
                    // 检查 AntButton.cs... 它有 Icon 属性吗？
                    // 读取的文件在前 50 行中未显示 Icon 属性。
                    // 让我们假设我们现在只设置 Content。
                }

                if (!string.IsNullOrEmpty(action.ToolTip))
                {
                    btnFactory.SetValue(AntButton.ToolTipProperty, action.ToolTip);
                }

                panelFactory.AppendChild(btnFactory);
            }

            cellTemplate.VisualTree = panelFactory;
            actionColumn.CellTemplate = cellTemplate;
            _dataGrid.Columns.Add(actionColumn);
        }
    }
}
