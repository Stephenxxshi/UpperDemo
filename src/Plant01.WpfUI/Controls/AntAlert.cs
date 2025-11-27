using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Plant01.WpfUI.Controls
{
    public enum AlertType
    {
        Success,
        Info,
        Warning,
        Error
    }

    public class AntAlert : ContentControl
    {
        static AntAlert()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntAlert), new FrameworkPropertyMetadata(typeof(AntAlert)));
        }

        public AntAlert()
        {
            CloseCommand = new SimpleCommand(OnClose);
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(AlertType), typeof(AntAlert), new PropertyMetadata(AlertType.Info));

        public AlertType Type
        {
            get => (AlertType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(AntAlert), new PropertyMetadata(null));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(AntAlert), new PropertyMetadata(null));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            nameof(ShowIcon), typeof(bool), typeof(AntAlert), new PropertyMetadata(false));

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public static readonly DependencyProperty ClosableProperty = DependencyProperty.Register(
            nameof(Closable), typeof(bool), typeof(AntAlert), new PropertyMetadata(false));

        public bool Closable
        {
            get => (bool)GetValue(ClosableProperty);
            set => SetValue(ClosableProperty, value);
        }

        public static readonly DependencyProperty BannerProperty = DependencyProperty.Register(
            nameof(Banner), typeof(bool), typeof(AntAlert), new PropertyMetadata(false));

        public bool Banner
        {
            get => (bool)GetValue(BannerProperty);
            set => SetValue(BannerProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(object), typeof(AntAlert), new PropertyMetadata(null));

        public object Icon
        {
            get => (object)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public ICommand CloseCommand { get; }

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
            nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AntAlert));

        public event RoutedEventHandler Closed
        {
            add => AddHandler(ClosedEvent, value);
            remove => RemoveHandler(ClosedEvent, value);
        }

        private void OnClose(object parameter)
        {
            this.Visibility = Visibility.Collapsed;
            RaiseEvent(new RoutedEventArgs(ClosedEvent));
        }

        private class SimpleCommand : ICommand
        {
            private readonly Action<object> _action;
            public SimpleCommand(Action<object> action) => _action = action;
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _action(parameter!);
        }
    }
}
