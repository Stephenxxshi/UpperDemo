using System.Collections.Generic;
using System.Windows.Input;

namespace Plant01.Core.Models.DynamicList
{
    public enum SearchControlType
    {
        Text,
        Select,
        Date,
        DateRange
    }

    public enum RowActionDisplayMode
    {
        Text,
        Icon,
        Both
    }

    public enum ColumnWidthType
    {
        Star,
        Auto,
        Pixel
    }

    public class SearchFieldConfig
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public SearchControlType Type { get; set; } = SearchControlType.Text;
        public IEnumerable<object>? Options { get; set; }
        public string? DisplayMemberPath { get; set; }
        public string? SelectedValuePath { get; set; }
        public string? SecondaryKey { get; set; }
        public object? DefaultValue { get; set; }
        public bool ShowLabel { get; set; } = false;
    }

    public class ColumnConfig
    {
        public string Header { get; set; } = string.Empty;
        public string BindingPath { get; set; } = string.Empty;
        
        /// <summary>
        /// ���ؿ��ȡ����Ϊ null����ʹ�� WidthType��
        /// </summary>
        public double? Width { get; set; }
        
        /// <summary>
        /// "Auto"��"Star"����� Width Ϊ null ʱ��Ĭ��ֵ����"Pixel"
        /// </summary>
        public ColumnWidthType WidthType { get; set; } = ColumnWidthType.Pixel;
        
        public object? Converter { get; set; }
        public string? ConverterName { get; set; }
        public object? ConverterParameter { get; set; }
        public string? StringFormat { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsSortable { get; set; } = true;
        public bool IsFrozen { get; set; } = false;
    }

    public class RowActionConfig
    {
        public string Label { get; set; } = string.Empty;
        public object? Icon { get; set; }
        public ICommand? Command { get; set; }
        public RowActionDisplayMode DisplayMode { get; set; } = RowActionDisplayMode.Text;
        public string? ToolTip { get; set; }
        /// <summary>
        /// DesignTokenKeys 中的属性名称，例如 "ErrorColor", "WarningColor"
        /// </summary>
        public string? ColorToken { get; set; }

        /// <summary>
        /// 使用枚举强类型指定颜色 Token
        /// </summary>
        public DesignTokenType? ColorTokenType { get; set; }
    }

    public class ListConfiguration
    {   
        public List<SearchFieldConfig> SearchFields { get; set; } = new();
        public List<ColumnConfig> Columns { get; set; } = new();
        public List<RowActionConfig> RowActions { get; set; } = new();
        public string ActionItemMargin { get; set; } = "4,0,4,0";
        public bool IsActionColumnFrozen { get; set; } = false;
        
        /// <summary>
        /// 冻结左侧列的数量
        /// </summary>
        public int FrozenColumnCount { get; set; } = 0;
    }
}
