using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class SelectPage : UserControl
    {
        public SelectPage()
        {
            InitializeComponent();
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSelect == null || SizeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoSelect.Size = content switch
            {
                "Small" => AntSize.Small,
                "Default" => AntSize.Default,
                "Large" => AntSize.Large,
                _ => AntSize.Default
            };
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoSelect != null && sender is CheckBox checkBox)
            {
                DemoSelect.IsEnabled = checkBox.IsChecked != true;
            }
        }
    }
}
