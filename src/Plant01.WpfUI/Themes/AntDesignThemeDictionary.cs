using System.Windows;
using System.Windows.Media;
using Plant01.WpfUI.Helpers;

namespace Plant01.WpfUI.Themes
{
    public class AntDesignThemeDictionary : ResourceDictionary
    {
        private Color _primaryColor = Color.FromRgb(0x16, 0x77, 0xff); // Default Blue
        private ThemeType _themeType = ThemeType.Light;

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

        public AntDesignThemeDictionary()
        {
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            ThemeManager.ApplyThemeToDictionary(this, _primaryColor, _themeType);
        }
    }
}
