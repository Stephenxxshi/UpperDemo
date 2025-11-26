using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Plant01.WpfUI.Controls;

public class IconRadioButton : RadioButton
{
    static IconRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IconRadioButton), new FrameworkPropertyMetadata(typeof(IconRadioButton)));
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(string), typeof(IconRadioButton), new PropertyMetadata(string.Empty, null, CoerceIcon));

    private static object CoerceIcon(DependencyObject d, object baseValue)
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

    public string Icon
    {
        get { return (string)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register("IconSize", typeof(double), typeof(IconRadioButton), new PropertyMetadata(12.0));

    public double IconSize
    {
        get { return (double)GetValue(IconSizeProperty); }
        set { SetValue(IconSizeProperty, value); }
    }

    public static readonly DependencyProperty IconFontFamilyProperty =
        DependencyProperty.Register("IconFontFamily", typeof(FontFamily), typeof(IconRadioButton), new PropertyMetadata(new FontFamily("Segoe UI Symbol")));

    public FontFamily IconFontFamily
    {
        get { return (FontFamily)GetValue(IconFontFamilyProperty); }
        set { SetValue(IconFontFamilyProperty, value); }
    }

    public static readonly DependencyProperty IconPlacementProperty =
        DependencyProperty.Register("IconPlacement", typeof(IconPlacement), typeof(IconRadioButton), new PropertyMetadata(IconPlacement.Left));

    public IconPlacement IconPlacement
    {
        get { return (IconPlacement)GetValue(IconPlacementProperty); }
        set { SetValue(IconPlacementProperty, value); }
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(IconRadioButton), new PropertyMetadata(new CornerRadius(0)));

    public CornerRadius CornerRadius
    {
        get { return (CornerRadius)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    public static readonly DependencyProperty IconMarginProperty =
        DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(IconRadioButton), new PropertyMetadata(new Thickness(0, 0, 5, 0)));

    public Thickness IconMargin
    {
        get { return (Thickness)GetValue(IconMarginProperty); }
        set { SetValue(IconMarginProperty, value); }
    }
}
