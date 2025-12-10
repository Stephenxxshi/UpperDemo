using System.Globalization;
using System.Windows.Data;

using CoreModels = Plant01.Upper.Presentation.Core.Models.DynamicList;
using UIModels = Plant01.WpfUI.Models.DynamicList;

namespace Plant01.Upper.Wpf.con;

public class ListConfigurationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CoreModels.ListConfiguration coreConfig)
        {
            var uiConfig = new UIModels.ListConfiguration();

            // Map SearchFields
            foreach (var field in coreConfig.SearchFields)
            {
                uiConfig.SearchFields.Add(new UIModels.SearchFieldConfig
                {
                    Key = field.Key,
                    Label = field.Label,
                    Type = (UIModels.SearchControlType)(int)field.Type, // Assuming enums match
                    Options = field.Options,
                    DisplayMemberPath = field.DisplayMemberPath,
                    SelectedValuePath = field.SelectedValuePath,
                    SecondaryKey = field.SecondaryKey,
                    DefaultValue = field.DefaultValue
                });
            }

            // Map Columns
            foreach (var col in coreConfig.Columns)
            {
                uiConfig.Columns.Add(new UIModels.ColumnConfig
                {
                    Header = col.Header,
                    BindingPath = col.BindingPath,
                    Width = col.Width,
                    WidthType = col.WidthType,
                    ConverterName = col.ConverterName,
                    ConverterParameter = col.ConverterParameter,
                    StringFormat = col.StringFormat,
                    IsVisible = col.IsVisible
                });
            }

            // Map RowActions
            foreach (var action in coreConfig.RowActions)
            {
                uiConfig.RowActions.Add(new UIModels.RowActionConfig
                {
                    Label = action.Label,
                    Icon = action.Icon,
                    Command = action.Command,
                    DisplayMode = (UIModels.RowActionDisplayMode)(int)action.DisplayMode, // Assuming enums match
                    ToolTip = action.ToolTip
                });
            }

            return uiConfig;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
