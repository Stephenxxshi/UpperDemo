using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Plant01.WpfUI.Themes
{
    public class ColorToBrushConverter : IValueConverter
    {
        public static ColorToBrushConverter Instance = new ColorToBrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            if (value is string colorCode)
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
                }
                catch { }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
