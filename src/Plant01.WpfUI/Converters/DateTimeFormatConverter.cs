using System;
using System.Globalization;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class DateTimeFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is DateTime dateTime && values[1] is string format)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(format))
                        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", culture);

                    return dateTime.ToString(format, culture);
                }
                catch
                {
                    return dateTime.ToString(culture);
                }
            }
            return values[0]?.ToString() ?? string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
