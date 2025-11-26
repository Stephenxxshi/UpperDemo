using System.Windows;
using System.Windows.Media;
using Plant01.WpfUI.Controls;
using Plant01.WpfUI.Helpers;
using wpfuidemo.Views;

namespace wpfuidemo
{
    public partial class MainWindow : Window
    {
        private bool _isDark = false;
        private DensityType _currentDensity = DensityType.Default;
        private Color _primarySeed = Color.FromRgb(0x16, 0x77, 0xff); // Ant Design Blue

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme();
            NavigateTo("Dashboard");
        }

        private void ApplyTheme()
        {
            ThemeManager.ApplyTheme(_primarySeed, _isDark ? ThemeType.Dark : ThemeType.Light, _currentDensity);
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDark = !_isDark;
            ApplyTheme();
        }

        private void CompactMode_Click(object sender, RoutedEventArgs e)
        {
            _currentDensity = DensityType.Compact;
            ApplyTheme();
        }

        private void TouchMode_Click(object sender, RoutedEventArgs e)
        {
            _currentDensity = DensityType.Touch;
            ApplyTheme();
        }

        private void NavMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is AntMenuItem item && item.Tag is string tag)
            {
                NavigateTo(tag);
            }
        }

        private void NavigateTo(string tag)
        {
            if (MainContent == null) return;

            switch (tag)
            {
                case "Dashboard":
                    MainContent.Content = new DashboardPage();
                    break;
                case "Button":
                    MainContent.Content = new ButtonPage();
                    break;
                default:
                    // Keep current content or show placeholder
                    break;
            }
        }
    }
}