using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Plant01.WpfUI.Controls
{
    public class ColumnGroupItem : INotifyPropertyChanged
    {
        private double _width;
        private string? _header;
        private Visibility _visibility;

        public string? Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(nameof(Header)); }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(nameof(Width)); }
        }

        public Visibility Visibility
        {
            get => _visibility;
            set { _visibility = value; OnPropertyChanged(nameof(Visibility)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AntDataGrid : DataGrid
    {
        static AntDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntDataGrid), new FrameworkPropertyMetadata(typeof(AntDataGrid)));
        }

        public AntDataGrid()
        {
            ColumnGroupsSource = new ObservableCollection<ColumnGroupItem>();
            this.LayoutUpdated += AntDataGrid_LayoutUpdated;
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(AntSize), typeof(AntDataGrid), new PropertyMetadata(AntSize.Default));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        // Attached Property: ColumnGroup
        public static readonly DependencyProperty ColumnGroupProperty =
            DependencyProperty.RegisterAttached("ColumnGroup", typeof(string), typeof(AntDataGrid), new PropertyMetadata(null));

        public static string? GetColumnGroup(DependencyObject obj) => (string?)obj.GetValue(ColumnGroupProperty);
        public static void SetColumnGroup(DependencyObject obj, string? value) => obj.SetValue(ColumnGroupProperty, value);

        // DP: ShowColumnGroups
        public static readonly DependencyProperty ShowColumnGroupsProperty =
            DependencyProperty.Register(nameof(ShowColumnGroups), typeof(bool), typeof(AntDataGrid), new PropertyMetadata(false, OnShowColumnGroupsChanged));

        public bool ShowColumnGroups
        {
            get => (bool)GetValue(ShowColumnGroupsProperty);
            set => SetValue(ShowColumnGroupsProperty, value);
        }

        // DP: ColumnGroupsSource
        public static readonly DependencyProperty ColumnGroupsSourceProperty =
            DependencyProperty.Register(nameof(ColumnGroupsSource), typeof(ObservableCollection<ColumnGroupItem>), typeof(AntDataGrid), new PropertyMetadata(null));

        public ObservableCollection<ColumnGroupItem> ColumnGroupsSource
        {
            get => (ObservableCollection<ColumnGroupItem>)GetValue(ColumnGroupsSourceProperty);
            private set => SetValue(ColumnGroupsSourceProperty, value);
        }

        private static void OnShowColumnGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as AntDataGrid)?.UpdateColumnGroups();
        }

        private void AntDataGrid_LayoutUpdated(object? sender, EventArgs e)
        {
            if (ShowColumnGroups)
            {
                UpdateColumnGroups();
            }
        }

        private void UpdateColumnGroups()
        {
            if (Columns.Count == 0) return;

            var sortedColumns = Columns.OrderBy(c => c.DisplayIndex).ToList();
            var newGroups = new System.Collections.Generic.List<ColumnGroupItem>();

            ColumnGroupItem? currentGroup = null;

            foreach (var column in sortedColumns)
            {
                if (column.Visibility != Visibility.Visible) continue;

                string? groupName = GetColumnGroup(column);
                double colWidth = column.ActualWidth;

                if (currentGroup != null && currentGroup.Header == groupName)
                {
                    currentGroup.Width += colWidth;
                }
                else
                {
                    currentGroup = new ColumnGroupItem
                    {
                        Header = groupName,
                        Width = colWidth,
                        Visibility = string.IsNullOrEmpty(groupName) ? Visibility.Hidden : Visibility.Visible
                    };
                    newGroups.Add(currentGroup);
                }
            }

            // Sync with ObservableCollection to avoid full rebuild if possible, 
            // but for simplicity and correctness with widths, we'll just update properties or replace.
            // To prevent flickering, we try to match by index.
            
            if (ColumnGroupsSource.Count != newGroups.Count)
            {
                ColumnGroupsSource.Clear();
                foreach (var g in newGroups) ColumnGroupsSource.Add(g);
            }
            else
            {
                for (int i = 0; i < newGroups.Count; i++)
                {
                    var oldItem = ColumnGroupsSource[i];
                    var newItem = newGroups[i];
                    if (oldItem.Header != newItem.Header) oldItem.Header = newItem.Header;
                    if (Math.Abs(oldItem.Width - newItem.Width) > 0.1) oldItem.Width = newItem.Width;
                    if (oldItem.Visibility != newItem.Visibility) oldItem.Visibility = newItem.Visibility;
                }
            }
        }

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            var column = eventArgs.Column;
            var currentSortDirection = column.SortDirection;

            // Tri-state sorting: Asc -> Desc -> None -> Asc
            if (currentSortDirection == ListSortDirection.Descending)
            {
                eventArgs.Handled = true;
                column.SortDirection = null;
                Items.SortDescriptions.Clear();
                Items.Refresh();
            }
            else
            {
                base.OnSorting(eventArgs);
            }
        }
    }
}
