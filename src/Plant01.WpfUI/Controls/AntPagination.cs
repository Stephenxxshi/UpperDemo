using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plant01.WpfUI.Controls
{
    public class AntPagination : Control
    {
        static AntPagination()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntPagination), new FrameworkPropertyMetadata(typeof(AntPagination)));
        }

        public AntPagination()
        {
            PagingSource = new ObservableCollection<PaginationItem>();
            
            PageClickCommand = new SimpleCommand(OnPageClick);
            PrevPageCommand = new SimpleCommand(OnPrevPage);
            NextPageCommand = new SimpleCommand(OnNextPage);
            JumpPrevCommand = new SimpleCommand(OnJumpPrev);
            JumpNextCommand = new SimpleCommand(OnJumpNext);

            Loaded += AntPagination_Loaded;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_QuickJumper") is TextBox quickJumper)
            {
                quickJumper.KeyDown += QuickJumper_KeyDown;
            }
        }

        private void QuickJumper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb)
            {
                if (int.TryParse(tb.Text, out int page))
                {
                    SetCurrentPage(page);
                    tb.Text = string.Empty; // Optional: clear or keep
                }
            }
        }

        private void AntPagination_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePagingSource();
        }

        #region Dependency Properties

        public static readonly DependencyProperty CurrentProperty =
            DependencyProperty.Register(nameof(Current), typeof(int), typeof(AntPagination), new PropertyMetadata(1, OnPagingPropertyChanged));

        public int Current
        {
            get => (int)GetValue(CurrentProperty);
            set => SetValue(CurrentProperty, value);
        }

        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(nameof(Total), typeof(int), typeof(AntPagination), new PropertyMetadata(0, OnPagingPropertyChanged));

        public int Total
        {
            get => (int)GetValue(TotalProperty);
            set => SetValue(TotalProperty, value);
        }

        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(AntPagination), new PropertyMetadata(10, OnPagingPropertyChanged));

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }

        public static readonly DependencyProperty ShowSizeChangerProperty =
            DependencyProperty.Register(nameof(ShowSizeChanger), typeof(bool), typeof(AntPagination), new PropertyMetadata(false));

        public bool ShowSizeChanger
        {
            get => (bool)GetValue(ShowSizeChangerProperty);
            set => SetValue(ShowSizeChangerProperty, value);
        }

        public static readonly DependencyProperty ShowQuickJumperProperty =
            DependencyProperty.Register(nameof(ShowQuickJumper), typeof(bool), typeof(AntPagination), new PropertyMetadata(false));

        public bool ShowQuickJumper
        {
            get => (bool)GetValue(ShowQuickJumperProperty);
            set => SetValue(ShowQuickJumperProperty, value);
        }

        public static readonly DependencyProperty PageSizeOptionsProperty =
            DependencyProperty.Register(nameof(PageSizeOptions), typeof(IEnumerable<int>), typeof(AntPagination), new PropertyMetadata(new int[] { 10, 20, 50, 100 }));

        public IEnumerable<int> PageSizeOptions
        {
            get => (IEnumerable<int>)GetValue(PageSizeOptionsProperty);
            set => SetValue(PageSizeOptionsProperty, value);
        }

        private static readonly DependencyPropertyKey PageCountPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(PageCount), typeof(int), typeof(AntPagination), new PropertyMetadata(0));

        public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

        public int PageCount
        {
            get => (int)GetValue(PageCountProperty);
            private set => SetValue(PageCountPropertyKey, value);
        }

        #endregion

        #region Events

        public static readonly RoutedEvent PageChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(PageChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<int>), typeof(AntPagination));

        public event RoutedPropertyChangedEventHandler<int> PageChanged
        {
            add => AddHandler(PageChangedEvent, value);
            remove => RemoveHandler(PageChangedEvent, value);
        }

        #endregion

        #region Internal Source

        public ObservableCollection<PaginationItem> PagingSource { get; }

        #endregion

        #region Commands

        public ICommand PageClickCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand JumpPrevCommand { get; }
        public ICommand JumpNextCommand { get; }

        #endregion

        private static void OnPagingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AntPagination pagination)
            {
                pagination.UpdatePagingSource();
            }
        }

        private void UpdatePagingSource()
        {
            if (PageSize <= 0) return;

            int count = (int)Math.Ceiling((double)Total / PageSize);
            PageCount = count;

            // Clamp Current
            if (Current < 1 && count > 0) Current = 1;
            if (Current > count) Current = count;

            PagingSource.Clear();

            if (count <= 0) return;

            // Logic for generating items
            // Ant Design Logic:
            // If <= 7 pages, show all.
            // If > 7:
            //   Always show 1 and Last.
            //   If Current is close to start (<=4): 1 2 3 4 5 ... Last
            //   If Current is close to end (>= Last-3): 1 ... L-4 L-3 L-2 L-1 L
            //   Else: 1 ... C-2 C-1 C C+1 C+2 ... L

            if (count <= 7)
            {
                for (int i = 1; i <= count; i++)
                {
                    PagingSource.Add(new PaginationItem(i, i == Current));
                }
            }
            else
            {
                // Always add first
                PagingSource.Add(new PaginationItem(1, Current == 1));

                if (Current <= 4)
                {
                    for (int i = 2; i <= 5; i++)
                    {
                        PagingSource.Add(new PaginationItem(i, i == Current));
                    }
                    PagingSource.Add(new PaginationItem("...", PaginationItemType.JumpNext));
                }
                else if (Current >= count - 3)
                {
                    PagingSource.Add(new PaginationItem("...", PaginationItemType.JumpPrev));
                    for (int i = count - 4; i < count; i++)
                    {
                        PagingSource.Add(new PaginationItem(i, i == Current));
                    }
                }
                else
                {
                    PagingSource.Add(new PaginationItem("...", PaginationItemType.JumpPrev));
                    for (int i = Current - 2; i <= Current + 2; i++)
                    {
                        PagingSource.Add(new PaginationItem(i, i == Current));
                    }
                    PagingSource.Add(new PaginationItem("...", PaginationItemType.JumpNext));
                }

                // Always add last
                PagingSource.Add(new PaginationItem(count, Current == count));
            }
        }

        private void OnPageClick(object? parameter)
        {
            if (parameter is int page)
            {
                SetCurrentPage(page);
            }
        }

        private void OnPrevPage(object? parameter)
        {
            if (Current > 1) SetCurrentPage(Current - 1);
        }

        private void OnNextPage(object? parameter)
        {
            if (Current < PageCount) SetCurrentPage(Current + 1);
        }

        private void OnJumpPrev(object? parameter)
        {
            SetCurrentPage(Math.Max(1, Current - 5));
        }

        private void OnJumpNext(object? parameter)
        {
            SetCurrentPage(Math.Min(PageCount, Current + 5));
        }

        private void SetCurrentPage(int newPage)
        {
            if (Current != newPage)
            {
                int oldPage = Current;
                Current = newPage;
                RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldPage, newPage, PageChangedEvent));
            }
        }
    }

    public enum PaginationItemType
    {
        Page,
        JumpPrev,
        JumpNext
    }

    public class PaginationItem
    {
        public string Display { get; }
        public int Page { get; }
        public bool IsCurrent { get; }
        public PaginationItemType Type { get; }

        public PaginationItem(int page, bool isCurrent)
        {
            Display = page.ToString();
            Page = page;
            IsCurrent = isCurrent;
            Type = PaginationItemType.Page;
        }

        public PaginationItem(string display, PaginationItemType type)
        {
            Display = display;
            Type = type;
            Page = -1;
            IsCurrent = false;
        }
    }

    // Simple Command Implementation since we don't have Mvvm Core dependency
    public class SimpleCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public SimpleCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);
    }
}
