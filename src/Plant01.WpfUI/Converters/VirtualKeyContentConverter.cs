using System;
using System.Globalization;
using System.Windows.Data;

namespace Plant01.WpfUI.Converters
{
    public class VirtualKeyContentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0]: Base character (string) - usually from CommandParameter
            // values[1]: IsShiftEnabled (bool)
            // values[2]: IsCapsLock (bool)

            if (values.Length < 3 || values[0] == null)
                return "";

            string? baseChar = values[0].ToString();
            if (string.IsNullOrEmpty(baseChar))
                return "";

            bool isShift = values[1] is bool b1 && b1;
            bool isCaps = values[2] is bool b2 && b2;

            // If it's not a letter, Shift usually changes it to a symbol (e.g. 1 -> !)
            // But for simplicity in this "simple" enhancement, we might just handle casing for letters first.
            // Handling symbols requires a mapping table.

            bool isUpper = isShift ^ isCaps; // XOR: Shift=T, Caps=F -> T; Shift=T, Caps=T -> F (usually? actually Shift overrides Caps for letters)
            
            // Standard behavior:
            // Caps Lock only affects Letters.
            // Shift affects Letters (inverts Caps) and Numbers/Symbols.

            if (baseChar.Length == 1 && char.IsLetter(baseChar[0]))
            {
                return isUpper ? baseChar.ToUpper() : baseChar.ToLower();
            }

            // Simple symbol mapping for standard US layout
            if (isShift)
            {
                return baseChar switch
                {
                    "1" => "!",
                    "2" => "@",
                    "3" => "#",
                    "4" => "$",
                    "5" => "%",
                    "6" => "^",
                    "7" => "&",
                    "8" => "*",
                    "9" => "(",
                    "0" => ")",
                    "-" => "_",
                    "=" => "+",
                    "[" => "{",
                    "]" => "}",
                    "\\" => "|",
                    ";" => ":",
                    "'" => "\"",
                    "," => "<",
                    "." => ">",
                    "/" => "?",
                    "`" => "~",
                    _ => baseChar
                };
            }

            return baseChar;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
