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
    }

    public class ColumnConfig
    {
        public string Header { get; set; } = string.Empty;
        public string BindingPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Pixel width. If null, uses WidthType.
        /// </summary>
        public double? Width { get; set; }
        
        /// <summary>
        /// "Auto", "Star" (default if Width is null), "Pixel"
        /// </summary>
        public ColumnWidthType WidthType { get; set; } = ColumnWidthType.Star;
        
        public string? ConverterName { get; set; }
        public object? ConverterParameter { get; set; }
        public string? StringFormat { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class RowActionConfig
    {
        public string Label { get; set; } = string.Empty;
        public object? Icon { get; set; }
        public ICommand? Command { get; set; }
        public RowActionDisplayMode DisplayMode { get; set; } = RowActionDisplayMode.Text;
        public string? ToolTip { get; set; }
    }

    public class ListConfiguration
    {   
        public List<SearchFieldConfig> SearchFields { get; set; } = new();
        public List<ColumnConfig> Columns { get; set; } = new();
        public List<RowActionConfig> RowActions { get; set; } = new();
    }
}
