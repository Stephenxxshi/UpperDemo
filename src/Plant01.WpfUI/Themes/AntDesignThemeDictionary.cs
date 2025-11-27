using System;
using System.Windows;
using System.Windows.Media;
using Plant01.WpfUI.Helpers;

namespace Plant01.WpfUI.Themes
{
    public class AntDesignThemeDictionary : ResourceDictionary
    {
        private Color _primaryColor = Color.FromRgb(0x16, 0x77, 0xff); // Default Blue
        private ThemeType _themeType = ThemeType.Light;
        private DensityType _density = DensityType.Default;
        private Color? _bodyBackground = null;

        public Color PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                UpdateTheme();
            }
        }

        public ThemeType ThemeType
        {
            get => _themeType;
            set
            {
                _themeType = value;
                UpdateTheme();
            }
        }

        public DensityType Density
        {
            get => _density;
            set
            {
                _density = value;
                UpdateTheme();
            }
        }

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
}
