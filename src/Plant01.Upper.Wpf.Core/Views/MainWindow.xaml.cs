using CommunityToolkit.Mvvm.Input;

using Plant01.Upper.Presentation.Core.ViewModels;
using Plant01.WpfUI.Controls;
using Plant01.WpfUI.Helpers;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Plant01.Upper.Wpf.Core;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AntWindow
{
    private bool _isDark = false;
    private DensityType _currentDensity = DensityType.Default;
    private Color _primarySeed = Color.FromRgb(0x16, 0x77, 0xff); // Ant Design Blue

    public ICommand CloseAppCommand { get; set; }
    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register(
            nameof(CurrentTime),
            typeof(DateTime),
            typeof(MainWindow),
            new PropertyMetadata(DateTime.Now)
        );
    public MainWindow(ShellViewModel shellViewModel)
    {
        CloseAppCommand = new RelayCommand(OnCloseApp);
        DataContext = shellViewModel;
        InitializeComponent();
        Dispatcher.Invoke(Clock);
        ApplyTheme();
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


    private void OnCloseApp()
    {
        var modal = new AntModal
        {
            Title = "确认退出",
            Content = "确定要退出系统吗？",
            Mask = AntModalMask.Blur,
            Owner = this,
            Width = 300,
            Height = 180,
            OkText = "确定",
            CancelText = "取消"
        };

        if (modal.ShowDialog() == true)
        {
            Application.Current.Shutdown();
        }
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

    private void Clock()
    {
        var timer = new DispatcherTimer();
        timer.Tick += (s, e) =>
        {
            CurrentTime = DateTime.Now;
        };
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Start();
    }
}