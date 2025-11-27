using System;
using System.Globalization;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class AddOneConverter : IValueConverter
    {
        public static AddOneConverter Instance = new AddOneConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return i + 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
