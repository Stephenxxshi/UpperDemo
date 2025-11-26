using System;
using System.Windows;

namespace Plant01.WpfUI.Helpers
{
    public static class IconHelper
    {
        public static object CoerceIcon(DependencyObject d, object baseValue)
        {
            string? val = baseValue as string;
            if (string.IsNullOrEmpty(val)) return baseValue;

            // Handle HTML entity format like &#xe7c6;
            if (val.StartsWith("&#x", StringComparison.OrdinalIgnoreCase) && val.EndsWith(";"))
            {
                try
                {
                    string hex = val.Substring(3, val.Length - 4);
                    int code = Convert.ToInt32(hex, 16);
                    return char.ConvertFromUtf32(code);
                }
                catch
                {
                    // If parsing fails, return original
                    return baseValue;
                }
            }

            return baseValue;
        }
    }
}
