using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class SpaceMarginConverter : IMultiValueConverter
    {
        public static readonly SpaceMarginConverter Instance = new SpaceMarginConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double size && values[1] is Orientation direction)
            {
                return direction == Orientation.Horizontal 
                    ? new Thickness(0, 0, size, 0) 
                    : new Thickness(0, 0, 0, size);
            }
            return new Thickness(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
