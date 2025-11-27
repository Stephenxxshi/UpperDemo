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

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            _primarySeed = e.NewValue;
            ApplyTheme();
        }

        private void Density_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox combo && combo.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag is string density)
            {
                switch (density)
                {
                    case "Compact":
                        _currentDensity = DensityType.Compact;
                        break;
                    case "Default":
                        _currentDensity = DensityType.Default;
                        break;
                    case "Touch":
                        _currentDensity = DensityType.Touch;
                        break;
                }
                ApplyTheme();
            }
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
                case "Input":
                    MainContent.Content = new InputPage();
                    break;
                case "Switch":
                    MainContent.Content = new SwitchPage();
                    break;
                case "Checkbox":
                    MainContent.Content = new CheckboxPage();
                    break;
                case "ColorPicker":
                    MainContent.Content = new ColorPickerPage();
                    break;
                case "DatePicker":
                    MainContent.Content = new DatePickerPage();
                    break;
                case "Calendar":
                    MainContent.Content = new CalendarPage();
                    break;
                case "Select":
                    MainContent.Content = new SelectPage();
                    break;
                case "Layout":
                    MainContent.Content = new LayoutPage();
                    break;
                case "Pagination":
                    MainContent.Content = new PaginationPage();
                    break;
                case "DataGrid":
                    MainContent.Content = new DataGridPage();
                    break;
                case "Empty":
                    MainContent.Content = new EmptyPage();
                    break;
                case "Timeline":
                    MainContent.Content = new TimelinePage();
                    break;
                case "Steps":
                    MainContent.Content = new StepsPage();
                    break;
                case "Alert":
                    MainContent.Content = new AlertPage();
                    break;
                case "Modal":
                    MainContent.Content = new DialogPage();
                    break;
                default:
                    // Keep current content or show placeholder
                    break;
            }
        }
    }
}