using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Text;
using Plant01.WpfUI.Models;

namespace Plant01.WpfUI.Controls;

public class AntLog : Control
{
    private ListBox? _listBox;
    private ICollectionView? _view;

    static AntLog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AntLog), new FrameworkPropertyMetadata(typeof(AntLog)));
    }

    public AntLog()
    {
        CopyCommand = new SimpleCommand(ExecuteCopy, CanExecuteCopy);
        ViewDetailCommand = new SimpleCommand(ExecuteViewDetail);
    }

    #region Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AntLog), new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(AntLog), new PropertyMetadata(string.Empty, OnFilterChanged));

    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.Register(nameof(AutoScroll), typeof(bool), typeof(AntLog), new PropertyMetadata(true));

    public bool AutoScroll
    {
        get => (bool)GetValue(AutoScrollProperty);
        set => SetValue(AutoScrollProperty, value);
    }

    public static readonly DependencyProperty IsPausedProperty =
        DependencyProperty.Register(nameof(IsPaused), typeof(bool), typeof(AntLog), new PropertyMetadata(false));

    public bool IsPaused
    {
        get => (bool)GetValue(IsPausedProperty);
        set => SetValue(IsPausedProperty, value);
    }

    public static readonly DependencyProperty IsWordWrapProperty =
        DependencyProperty.Register(nameof(IsWordWrap), typeof(bool), typeof(AntLog), new PropertyMetadata(false));

    public bool IsWordWrap
    {
        get => (bool)GetValue(IsWordWrapProperty);
        set => SetValue(IsWordWrapProperty, value);
    }

    public static readonly DependencyProperty ShowTimestampProperty =
        DependencyProperty.Register(nameof(ShowTimestamp), typeof(bool), typeof(AntLog), new PropertyMetadata(true));

    public bool ShowTimestamp
    {
        get => (bool)GetValue(ShowTimestampProperty);
        set => SetValue(ShowTimestampProperty, value);
    }

    public static readonly DependencyProperty DateFormatProperty =
        DependencyProperty.Register(nameof(DateFormat), typeof(string), typeof(AntLog), new PropertyMetadata("yyyy-MM-dd HH:mm:ss.fff"));

    public string DateFormat
    {
        get => (string)GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    public static readonly DependencyProperty MaxItemCountProperty =
        DependencyProperty.Register(nameof(MaxItemCount), typeof(int), typeof(AntLog), new PropertyMetadata(2000));

    public int MaxItemCount
    {
        get => (int)GetValue(MaxItemCountProperty);
        set => SetValue(MaxItemCountProperty, value);
    }

    #endregion

    #region Commands

    public static readonly DependencyProperty ClearCommandProperty =
        DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(AntLog), new PropertyMetadata(null));

    public ICommand ClearCommand
    {
        get => (ICommand)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public ICommand CopyCommand { get; }
    public ICommand ViewDetailCommand { get; }

    #endregion

    #region Filters

    public static readonly DependencyProperty ShowInfoProperty =
        DependencyProperty.Register(nameof(ShowInfo), typeof(bool), typeof(AntLog), new PropertyMetadata(true, OnFilterChanged));
    public bool ShowInfo { get => (bool)GetValue(ShowInfoProperty); set => SetValue(ShowInfoProperty, value); }

    public static readonly DependencyProperty ShowDebugProperty =
        DependencyProperty.Register(nameof(ShowDebug), typeof(bool), typeof(AntLog), new PropertyMetadata(true, OnFilterChanged));
    public bool ShowDebug { get => (bool)GetValue(ShowDebugProperty); set => SetValue(ShowDebugProperty, value); }

    public static readonly DependencyProperty ShowWarningProperty =
        DependencyProperty.Register(nameof(ShowWarning), typeof(bool), typeof(AntLog), new PropertyMetadata(true, OnFilterChanged));
    public bool ShowWarning { get => (bool)GetValue(ShowWarningProperty); set => SetValue(ShowWarningProperty, value); }

    public static readonly DependencyProperty ShowErrorProperty =
        DependencyProperty.Register(nameof(ShowError), typeof(bool), typeof(AntLog), new PropertyMetadata(true, OnFilterChanged));
    public bool ShowError { get => (bool)GetValue(ShowErrorProperty); set => SetValue(ShowErrorProperty, value); }

    #endregion

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _listBox = GetTemplateChild("PART_ListBox") as ListBox;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (AntLog)d;
        control.UpdateView();
        
        if (e.OldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= control.OnCollectionChanged;
        }
        if (e.NewValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += control.OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (AutoScroll && e.Action == NotifyCollectionChangedAction.Add && _listBox != null)
        {
            // 使用 Dispatcher 延迟执行滚动，确保 UI 已经更新且 VirtualizingStackPanel 准备就绪
            // 避免 "itemIndex must be less than 0" 异常
            Dispatcher.InvokeAsync(() =>
            {
                if (_listBox.Items.Count > 0)
                {
                    var lastItem = _listBox.Items[_listBox.Items.Count - 1];
                    if (lastItem != null)
                    {
                        _listBox.ScrollIntoView(lastItem);
                    }
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AntLog)d)._view?.Refresh();
    }

    private void UpdateView()
    {
        if (ItemsSource == null) 
        {
            _view = null;
            return;
        }

        _view = CollectionViewSource.GetDefaultView(ItemsSource);
        _view.Filter = FilterItem;
    }

    private bool FilterItem(object obj)
    {
        if (obj is not LogItem item) return true;

        // Level Filter
        bool levelMatch = item.Severity switch
        {
            LogSeverity.Info => ShowInfo,
            LogSeverity.Debug => ShowDebug,
            LogSeverity.Warning => ShowWarning,
            LogSeverity.Error => ShowError,
            _ => true
        };

        if (!levelMatch) return false;

        // Text Filter
        if (string.IsNullOrEmpty(FilterText)) return true;
        
        return (item.Message?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (item.Source?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private void ExecuteCopy(object? parameter)
    {
        if (parameter is LogItem item)
        {
            Clipboard.SetText(FormatLogItem(item));
        }
        else if (_listBox?.SelectedItem is LogItem selectedItem)
        {
            Clipboard.SetText(FormatLogItem(selectedItem));
        }
    }

    private bool CanExecuteCopy(object? parameter)
    {
        return parameter is LogItem || _listBox?.SelectedItem is LogItem;
    }

    private string FormatLogItem(LogItem item)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{item.Timestamp.ToString(DateFormat)}] [{item.Severity}] {item.Source}");
        sb.AppendLine(item.Message);
        if (!string.IsNullOrEmpty(item.Exception))
        {
            sb.AppendLine(item.Exception);
        }
        return sb.ToString();
    }

    private void ExecuteViewDetail(object? parameter)
    {
        var item = parameter as LogItem ?? _listBox?.SelectedItem as LogItem;
        if (item == null) return;

        var window = new Window
        {
            Title = "Log Details",
            Width = 600,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = new TextBox
            {
                Text = FormatLogItem(item),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(10),
                FontFamily = new System.Windows.Media.FontFamily("Consolas")
            }
        };
        window.ShowDialog();
    }

    private class SimpleCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public SimpleCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);
        
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}