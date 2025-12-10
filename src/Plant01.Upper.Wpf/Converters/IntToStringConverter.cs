using System;
using System.Globalization;
using System.Windows.Data;

namespace Plant01.Upper.Wpf.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
            {
                return 0; // Default value for empty input
            }

            if (int.TryParse(value as string, out int result))
            {
                return result;
            }

            // Return the current value or handle error
            return Binding.DoNothing;
        }
    }
}
