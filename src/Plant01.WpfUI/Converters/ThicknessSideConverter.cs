using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class ThicknessSideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness t && parameter is string side)
            {
                return side.ToLower() switch
                {
                    "header" => new Thickness(t.Left, t.Top, t.Right, 0),
                    "content" => new Thickness(t.Left, 0, t.Right, t.Bottom),
                    "top" => new Thickness(0, t.Top, 0, 0),
                    "bottom" => new Thickness(0, 0, 0, t.Bottom),
                    "left" => new Thickness(t.Left, 0, 0, 0),
                    "right" => new Thickness(0, 0, t.Right, 0),
                    _ => t
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
