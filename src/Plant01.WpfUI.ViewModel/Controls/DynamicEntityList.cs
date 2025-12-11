using Plant01.Core.Models.DynamicList;
using Plant01.WpfUI.Controls;
using Plant01.WpfUI.Themes;

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Plant01.WpfUI.ViewModel.Controls;

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
        DependencyProperty.Register(nameof(SearchContext), typeof(object), typeof(DynamicEntityList), new PropertyMetadata(null, OnSearchContextChanged));

    private static void OnSearchContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DynamicEntityList)d).RefreshView();
    }

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
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(DynamicEntityList), new PropertyMetadata(0, OnTotalCountChanged));

    private static void OnTotalCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DynamicEntityList)d).UpdateEmptyState();
    }

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

        if (_dataGrid != null)
        {
            _dataGrid.VerticalAlignment = VerticalAlignment.Top;
        }

        this.SizeChanged += DynamicEntityList_SizeChanged;

        RefreshView();
    }

    private void DynamicEntityList_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateDataGridMaxHeight();
    }

    private void UpdateDataGridMaxHeight()
    {
        if (_dataGrid == null) return;

        double availableHeight = this.ActualHeight;

        if (_searchPanel != null)
        {
            // 查找 SearchPanel 的容器（Border）
            // 在 Template 中，SearchPanel 在一个 Border 中，Border 在 Row 0
            // 我们向上查找 Border
            var parent = VisualTreeHelper.GetParent(_searchPanel) as FrameworkElement;
            while (parent != null && !(parent is Border))
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            if (parent != null)
            {
                availableHeight -= parent.ActualHeight;
                availableHeight -= parent.Margin.Top + parent.Margin.Bottom;
            }
        }
        
        if (_pagination != null)
        {
            availableHeight -= _pagination.ActualHeight;
            availableHeight -= _pagination.Margin.Top + _pagination.Margin.Bottom;
        }

        // 减去一些 Padding/Margin
        availableHeight -= this.Padding.Top + this.Padding.Bottom;

        if (availableHeight < 0) availableHeight = 0;

        _dataGrid.MaxHeight = availableHeight;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (DynamicEntityList)d;
        control.UpdateEmptyState();
    }

    private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DynamicEntityList)d).RefreshView();
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
        FrameworkElement control;
        // 创建绑定的辅助方法
        Binding CreateBinding(string path)
        {
            var binding = new Binding();
            binding.Source = this; // 使用控件自身作为源，这样当 SearchContext 变化时绑定会自动更新
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 如果 SearchContext 是字典或动态对象，我们可能需要索引器绑定
            // 假设 SearchContext 是 ViewModel 对象，其属性与 'Key' 匹配
            // 如果是字典，路径应该是 SearchContext[Key]
            if (SearchContext is IDictionary)
            {
                binding.Path = new PropertyPath($"SearchContext[{path}]");
            }
            else
            {
                binding.Path = new PropertyPath($"SearchContext.{path}");
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
                control = textBox;
                break;

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
                control = comboBox;
                break;

            case SearchControlType.Date:
                var datePicker = new AntDatePicker
                {
                    Placeholder = field.Label,
                    Width = 150
                };
                datePicker.SetBinding(AntDatePicker.SelectedDateProperty, CreateBinding(field.Key));
                control = datePicker;
                break;

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
                control = dateRange;
                break;

            default:
                control = new TextBlock { Text = $"Unknown Type: {field.Type}" };
                break;
        }

        if (field.ShowLabel)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(new TextBlock 
            { 
                Text = field.Label + "：", 
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            });
            panel.Children.Add(control);
            return panel;
        }

        return control;
    }

    private void GenerateColumns()
    {
        if (_dataGrid == null || Configuration == null) return;

        _dataGrid.Columns.Clear();
        _dataGrid.FrozenColumnCount = 0;

        var columns = new List<DataGridColumn>();
        int frozenCount = 0;

        foreach (var colConfig in Configuration.Columns)
        {
            if (!colConfig.IsVisible) continue;

            var column = new DataGridTextColumn
            {
                Header = colConfig.Header,
                Binding = new Binding(colConfig.BindingPath),
                CanUserSort = colConfig.IsSortable
            };

            // 设置单元格内容居中
            var elementStyle = new Style(typeof(TextBlock));
            elementStyle.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            elementStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            column.ElementStyle = elementStyle;

            if (!string.IsNullOrEmpty(colConfig.StringFormat))
            {
                column.Binding.StringFormat = colConfig.StringFormat;
            }

            if (colConfig.Converter is IValueConverter directConverter)
            {
                (column.Binding as Binding).Converter = directConverter;
                (column.Binding as Binding).ConverterParameter = colConfig.ConverterParameter;
            }
            else if (!string.IsNullOrEmpty(colConfig.ConverterName))
            {
                if (TryFindResource(colConfig.ConverterName) is IValueConverter converter)
                {
                    (column.Binding as Binding).Converter = converter;
                    (column.Binding as Binding).ConverterParameter = colConfig.ConverterParameter;
                }
            }

            // 宽度处理
            if (colConfig.WidthType == ColumnWidthType.Star)
            {
                 // 如果是 Star 类型，Width 值作为权重，默认为 1
                 double starValue = colConfig.Width ?? 1.0;
                 column.Width = new DataGridLength(starValue, DataGridLengthUnitType.Star);
            }
            else if (colConfig.Width.HasValue)
            {
                column.Width = new DataGridLength(colConfig.Width.Value, DataGridLengthUnitType.Pixel);
            }
            else
            {
                column.Width = colConfig.WidthType switch
                {
                    ColumnWidthType.Auto => DataGridLength.Auto,
                    ColumnWidthType.Pixel => DataGridLength.SizeToCells, // 后备
                    _ => new DataGridLength(1, DataGridLengthUnitType.Star)
                };
            }

            columns.Add(column);

            if (colConfig.IsFrozen)
            {
                frozenCount++;
            }
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
                
                // 动态设置颜色
                ComponentResourceKey? colorKey = null;

                // 优先使用枚举
                if (action.ColorTokenType.HasValue)
                {
                    var prop = typeof(DesignTokenKeys).GetProperty(action.ColorTokenType.Value.ToString(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (prop != null)
                    {
                        colorKey = prop.GetValue(null) as ComponentResourceKey;
                    }
                }
                // 其次使用字符串
                else if (!string.IsNullOrEmpty(action.ColorToken))
                {
                    var prop = typeof(DesignTokenKeys).GetProperty(action.ColorToken, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (prop != null)
                    {
                        colorKey = prop.GetValue(null) as ComponentResourceKey;
                    }
                }
                
                // 默认为 PrimaryColor
                if (colorKey == null)
                {
                    colorKey = DesignTokenKeys.PrimaryColor;
                }
                
                btnFactory.SetResourceReference(Control.ForegroundProperty, colorKey);
                
                btnFactory.SetValue(AntButton.ContentProperty, action.Label);
                btnFactory.SetValue(AntButton.CommandProperty, action.Command);
                btnFactory.SetBinding(AntButton.CommandParameterProperty, new Binding(".")); // 绑定到行项目
                
                // 解析 Margin
                Thickness margin = new Thickness(4, 0, 4, 0);
                if (!string.IsNullOrEmpty(Configuration.ActionItemMargin))
                {
                    try
                    {
                        var converter = new ThicknessConverter();
                        var converted = converter.ConvertFromString(Configuration.ActionItemMargin);
                        if (converted != null)
                        {
                            margin = (Thickness)converted;
                        }
                    }
                    catch { /* 忽略解析错误，使用默认值 */ }
                }
                btnFactory.SetValue(FrameworkElement.MarginProperty, margin);

                // 处理显示模式 (图标/文本)
                if (action.Icon != null)
                {
                    if (action.DisplayMode == RowActionDisplayMode.Icon)
                    {
                        btnFactory.SetValue(AntButton.IconProperty, action.Icon.ToString());
                        btnFactory.SetValue(AntButton.IconFontFamilyProperty, new FontFamily(new Uri("pack://application:,,,/Plant01.WpfUI;component/Assets/Fonts/"), "./#iconfont"));
                        btnFactory.SetValue(AntButton.ContentProperty, null);
                    }
                    else if (action.DisplayMode == RowActionDisplayMode.Both)
                    {
                        btnFactory.SetValue(AntButton.IconProperty, action.Icon.ToString());
                        btnFactory.SetValue(AntButton.IconFontFamilyProperty, new FontFamily(new Uri("pack://application:,,,/Plant01.WpfUI;component/Assets/Fonts/"), "./#iconfont"));
                    }
                }

                if (!string.IsNullOrEmpty(action.ToolTip))
                {
                    btnFactory.SetValue(AntButton.ToolTipProperty, action.ToolTip);
                }

                panelFactory.AppendChild(btnFactory);
            }

            cellTemplate.VisualTree = panelFactory;
            actionColumn.CellTemplate = cellTemplate;

            if (Configuration.IsActionColumnFrozen)
            {
                columns.Insert(0, actionColumn);
                frozenCount++;
            }
            else
            {
                columns.Add(actionColumn);
            }
        }

        // 添加所有列
        foreach (var col in columns)
        {
            _dataGrid.Columns.Add(col);
        }

        // 应用全局冻结列设置
        if (Configuration.FrozenColumnCount > 0)
        {
            // 确保不小于已有的冻结列数（例如通过 IsFrozen 单独设置的）
            if (Configuration.FrozenColumnCount > frozenCount)
            {
                frozenCount = Configuration.FrozenColumnCount;
            }
        }

        // 安全检查
        if (frozenCount > _dataGrid.Columns.Count)
        {
            frozenCount = _dataGrid.Columns.Count;
        }

        _dataGrid.FrozenColumnCount = frozenCount;
    }
}
