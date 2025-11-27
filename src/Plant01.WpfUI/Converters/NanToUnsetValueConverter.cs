using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class NanToUnsetValueConverter : IValueConverter
    {
        public static readonly NanToUnsetValueConverter Instance = new NanToUnsetValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && double.IsNaN(d))
            {
                return DependencyProperty.UnsetValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
