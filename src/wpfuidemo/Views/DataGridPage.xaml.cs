using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class DataGridPage : UserControl
    {
        public DataGridPage()
        {
            InitializeComponent();
            
            var data = new List<object>();
            for (int i = 0; i < 20; i++)
            {
                data.Add(new { Name = $"User {i}", Age = 20 + i, Address = $"Address No. {i} Lake Park" });
            }
            MainGrid.ItemsSource = data;
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainGrid == null) return;
            switch (SizeCombo.SelectedIndex)
            {
                case 0: MainGrid.Size = AntSize.Small; break;
                case 1: MainGrid.Size = AntSize.Default; break;
                case 2: MainGrid.Size = AntSize.Large; break;
            }
        }

        private void GridLinesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainGrid == null) return;
            switch (GridLinesCombo.SelectedIndex)
            {
                case 0: MainGrid.GridLinesVisibility = DataGridGridLinesVisibility.None; break;
                case 1: MainGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal; break;
                case 2: MainGrid.GridLinesVisibility = DataGridGridLinesVisibility.Vertical; break;
                case 3: MainGrid.GridLinesVisibility = DataGridGridLinesVisibility.All; break;
            }
        }

        private void HeadersCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainGrid == null) return;
            switch (HeadersCombo.SelectedIndex)
            {
                case 0: MainGrid.HeadersVisibility = DataGridHeadersVisibility.All; break;
                case 1: MainGrid.HeadersVisibility = DataGridHeadersVisibility.Column; break;
                case 2: MainGrid.HeadersVisibility = DataGridHeadersVisibility.Row; break;
                case 3: MainGrid.HeadersVisibility = DataGridHeadersVisibility.None; break;
            }
        }

        private void HeaderGroupsCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainGrid == null) return;
            MainGrid.ShowColumnGroups = HeaderGroupsCheck.IsChecked == true;
        }

        private void StripedRowsCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainGrid == null) return;
            MainGrid.AlternationCount = (StripedRowsCheck.IsChecked == true) ? 2 : 0;
        }

        private void ReadOnlyCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainGrid == null) return;
            MainGrid.IsReadOnly = ReadOnlyCheck.IsChecked == true;
        }

        private void MultiSelectCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainGrid == null) return;
            MainGrid.SelectionMode = (MultiSelectCheck.IsChecked == true) ? DataGridSelectionMode.Extended : DataGridSelectionMode.Single;
        }

        private void ResizeCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainGrid == null) return;
            MainGrid.CanUserResizeColumns = ResizeCheck.IsChecked == true;
        }

        private void SortCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainGrid == null) return;
            MainGrid.CanUserSortColumns = SortCheck.IsChecked == true;
        }
    }
}
