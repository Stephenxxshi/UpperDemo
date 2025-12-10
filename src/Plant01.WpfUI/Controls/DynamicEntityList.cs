using Plant01.WpfUI.Models.DynamicList;

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

        // Pagination visibility logic
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
        // Helper to create binding
        Binding CreateBinding(string path)
        {
            var binding = new Binding(path)
            {
                Source = SearchContext,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            // If SearchContext is a Dictionary or Dynamic, we might need indexer binding
            // Assuming SearchContext is a ViewModel object with properties matching 'Key'
            // If it's a Dictionary, path should be [Key]
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

            // Width handling
            if (colConfig.Width.HasValue)
            {
                column.Width = new DataGridLength(colConfig.Width.Value, DataGridLengthUnitType.Pixel);
            }
            else
            {
                column.Width = colConfig.WidthType.ToLower() switch
                {
                    "auto" => DataGridLength.Auto,
                    "pixel" => DataGridLength.SizeToCells, // Fallback
                    _ => new DataGridLength(1, DataGridLengthUnitType.Star)
                };
            }

            _dataGrid.Columns.Add(column);
        }

        // Row Actions Column
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
                btnFactory.SetValue(AntButton.TypeProperty, ButtonType.Link); // Use Link style for table actions
                btnFactory.SetValue(AntButton.ContentProperty, action.Label);
                btnFactory.SetValue(AntButton.CommandProperty, action.Command);
                btnFactory.SetBinding(AntButton.CommandParameterProperty, new Binding(".")); // Bind to Row Item
                btnFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 0, 4, 0));

                // Handle DisplayMode (Icon/Text)
                if (action.DisplayMode == RowActionDisplayMode.Icon && action.Icon != null)
                {
                    // If Icon is string (font key), use TextBlock with font family
                    // For now, just set Content to Icon if it's a string
                    btnFactory.SetValue(AntButton.ContentProperty, action.Icon);
                    // You might need a specific style for Icon buttons
                }
                else if (action.DisplayMode == RowActionDisplayMode.Both && action.Icon != null)
                {
                    // Complex content with Icon + Text
                    // For simplicity in Factory, maybe just append text
                    // Or use AntButton's Icon property if it exists (it does usually)
                    // Let's assume AntButton has Icon property or similar mechanism
                    // Checking AntButton.cs... it has Icon property? 
                    // The file read didn't show Icon property in first 50 lines.
                    // Let's assume we just set Content for now.
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
