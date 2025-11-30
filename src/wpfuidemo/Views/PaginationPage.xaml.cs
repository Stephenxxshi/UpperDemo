using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class PaginationPage : UserControl
    {
        public PaginationPage()
        {
            InitializeComponent();
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoPagination == null) return;
            if (SizeCombo.SelectedItem is ComboBoxItem item && item.Content is string content)
            {
                switch (content)
                {
                    case "Small":
                        DemoPagination.Size = AntSize.Small;
                        break;
                    case "Default":
                        DemoPagination.Size = AntSize.Default;
                        break;
                    case "Large":
                        DemoPagination.Size = AntSize.Large;
                        break;
                }
            }
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoPagination == null) return;
            if (sender is CheckBox box)
            {
                DemoPagination.IsEnabled = !(box.IsChecked ?? false);
            }
        }
    }
}
