using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class FontSizeChooserConverter : IMultiValueConverter
    {
        public static readonly FontSizeChooserConverter Instance = new FontSizeChooserConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                // Value 0: ContentFontSize (specific)
                // Value 1: FontSize (general/fallback)

                if (values[0] is double contentFontSize && !double.IsNaN(contentFontSize))
                {
                    return contentFontSize;
                }

                if (values[1] is double fontSize && !double.IsNaN(fontSize))
                {
                    return fontSize;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
