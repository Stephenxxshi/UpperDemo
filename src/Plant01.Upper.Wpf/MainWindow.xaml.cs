using CommunityToolkit.Mvvm.Input;

using Plant01.Upper.Presentation.Core.ViewModels;
using Plant01.WpfUI.Controls;
using Plant01.WpfUI.Helpers;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Plant01.Upper.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AntWindow
{
    private bool _isDark = false;
    private DensityType _currentDensity = DensityType.Default;
    private Color _primarySeed = Color.FromRgb(0x16, 0x77, 0xff); // Ant Design Blue

    public ICommand CloseAppCommand { get; set; }
    public DateTime CurrentTime { get; set; }
    public MainWindow(ShellViewModel shellViewModel)
    {
        CloseAppCommand = new RelayCommand(OnCloseApp);
        DataContext = shellViewModel;
        Clock();
        InitializeComponent();
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