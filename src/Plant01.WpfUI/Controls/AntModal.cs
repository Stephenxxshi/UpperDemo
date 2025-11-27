using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Plant01.WpfUI.Controls
{
    public enum AntModalMask
    {
        None,
        Blur,
        Dim
    }

    public class AntModal : Window
    {
        private Window? _owner;
        private Effect? _originalEffect;
        private Window? _dimWindow;

        static AntModal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntModal), new FrameworkPropertyMetadata(typeof(AntModal)));
        }

        public AntModal()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));

            OkCommand = new SimpleCommand(OnOkClick);
            CancelCommand = new SimpleCommand(OnCancelClick);

            Loaded += AntModal_Loaded;
            Closed += AntModal_Closed;
        }

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
            nameof(Mask), typeof(AntModalMask), typeof(AntModal), new PropertyMetadata(AntModalMask.Blur));

        public AntModalMask Mask
        {
            get => (AntModalMask)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        private void AntModal_Loaded(object sender, RoutedEventArgs e)
        {
            _owner = Owner ?? Application.Current.MainWindow;
            if (_owner == null) return;

            if (Mask == AntModalMask.Blur)
            {
                _originalEffect = _owner.Effect;
                _owner.Effect = new BlurEffect { Radius = 10 };
            }
            else if (Mask == AntModalMask.Dim)
            {
                _dimWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                    ShowInTaskbar = false,
                    Owner = _owner,
                    Width = _owner.ActualWidth,
                    Height = _owner.ActualHeight,
                    Left = _owner.Left,
                    Top = _owner.Top,
                    WindowState = _owner.WindowState == WindowState.Maximized ? WindowState.Maximized : WindowState.Normal
                };
                _dimWindow.Show();
                this.Activate();
            }
        }

        private void AntModal_Closed(object? sender, EventArgs e)
        {
            if (_owner != null && Mask == AntModalMask.Blur)
            {
                _owner.Effect = _originalEffect;
            }
            
            if (_dimWindow != null)
            {
                _dimWindow.Close();
                _dimWindow = null;
            }
        }

        private void OnCloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        public static readonly DependencyProperty OkTextProperty = DependencyProperty.Register(
            nameof(OkText), typeof(string), typeof(AntModal), new PropertyMetadata("OK"));

        public string OkText
        {
            get => (string)GetValue(OkTextProperty);
            set => SetValue(OkTextProperty, value);
        }

        public static readonly DependencyProperty CancelTextProperty = DependencyProperty.Register(
            nameof(CancelText), typeof(string), typeof(AntModal), new PropertyMetadata("Cancel"));

        public string CancelText
        {
            get => (string)GetValue(CancelTextProperty);
            set => SetValue(CancelTextProperty, value);
        }

        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(
            nameof(Footer), typeof(object), typeof(AntModal), new PropertyMetadata(null));

        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        // Events for buttons
        public event RoutedEventHandler? OkClick;
        public event RoutedEventHandler? CancelClick;

        public void OnOkClick()
        {
            OkClick?.Invoke(this, new RoutedEventArgs());
            this.DialogResult = true;
            this.Close();
        }

        public void OnCancelClick()
        {
            CancelClick?.Invoke(this, new RoutedEventArgs());
            this.DialogResult = false;
            this.Close();
        }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private class SimpleCommand : ICommand
        {
            private readonly Action _action;
            public SimpleCommand(Action action) => _action = action;
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _action();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try
            {
                this.DragMove();
            }
            catch { }
        }
    }
}
