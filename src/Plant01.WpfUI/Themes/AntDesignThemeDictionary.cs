using Plant01.WpfUI.Helpers;

using System.Windows;
using System.Windows.Media;

namespace Plant01.WpfUI.Themes;

/// <summary>
/// 加载并应用 Ant Design 主题的资源字典
/// </summary>
public class AntDesignThemeDictionary : ResourceDictionary
{
    private Color _primaryColor = Color.FromRgb(0x16, 0x77, 0xff); // Default Blue
    public Color PrimaryColor
    {
        get => _primaryColor;
        set
        {
            _primaryColor = value;
            UpdateTheme();
        }
    }

    private ThemeType _themeType = ThemeType.Light;
    public ThemeType ThemeType
    {
        get => _themeType;
        set
        {
            _themeType = value;
            UpdateTheme();
        }
    }

    private DensityType _density = DensityType.Default;
    public DensityType Density
    {
        get => _density;
        set
        {
            _density = value;
            UpdateTheme();
        }
    }

    private Color? _bodyBackground = null;
    public Color? BodyBackground
    {
        get => _bodyBackground;
        set
        {
            _bodyBackground = value;
            UpdateTheme();
        }
    }

    public AntDesignThemeDictionary()
    {
        MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Plant01.WpfUI;component/Themes/Generic.xaml", UriKind.Relative) });
        UpdateTheme();
    }

    private void UpdateTheme()
    {
        ThemeManager.ApplyThemeToDictionary(this, _primaryColor, _themeType, _density, _bodyBackground);
    }
}
