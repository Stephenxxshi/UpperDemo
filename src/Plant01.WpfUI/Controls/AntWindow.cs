using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Plant01.WpfUI.Controls;

public class AntWindow : Window
{
    static AntWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AntWindow), new FrameworkPropertyMetadata(typeof(AntWindow)));
    }

    public AntWindow()
    {
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
    }

    public static readonly DependencyProperty NonClientAreaContentProperty = DependencyProperty.Register(
        nameof(NonClientAreaContent), typeof(object), typeof(AntWindow), new PropertyMetadata(null));

    public object NonClientAreaContent
    {
        get => GetValue(NonClientAreaContentProperty);
        set => SetValue(NonClientAreaContentProperty, value);
    }

    public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMinimizeButton), typeof(bool), typeof(AntWindow), new PropertyMetadata(true));

    public bool ShowMinimizeButton
    {
        get => (bool)GetValue(ShowMinimizeButtonProperty);
        set => SetValue(ShowMinimizeButtonProperty, value);
    }

    public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMaximizeButton), typeof(bool), typeof(AntWindow), new PropertyMetadata(true));

    public bool ShowMaximizeButton
    {
        get => (bool)GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand), typeof(ICommand), typeof(AntWindow), new PropertyMetadata(null));

    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
        nameof(TitleBarHeight), typeof(double), typeof(AntWindow), new PropertyMetadata(32.0, OnTitleBarHeightChanged));

    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    private static void OnTitleBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AntWindow window)
        {
            window.UpdateWindowChromeCaptionHeight((double)e.NewValue);
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        UpdateWindowChromeCaptionHeight(TitleBarHeight);
    }

    private void UpdateWindowChromeCaptionHeight(double height)
    {
        var chrome = WindowChrome.GetWindowChrome(this);
        if (chrome != null)
        {
            if (chrome.IsFrozen)
            {
                chrome = (WindowChrome)chrome.Clone();
                chrome.CaptionHeight = height;
                WindowChrome.SetWindowChrome(this, chrome);
            }
            else
            {
                chrome.CaptionHeight = height;
            }
        }
    }

    private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
    }

    private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = ResizeMode != ResizeMode.NoResize;
    }

    private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
    {
        if (CloseCommand != null && CloseCommand.CanExecute(null))
        {
            CloseCommand.Execute(null);
        }
        else
        {
            SystemCommands.CloseWindow(this);
        }
    }

    private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MaximizeWindow(this);
    }

    private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
    {
        SystemCommands.RestoreWindow(this);
    }
}
