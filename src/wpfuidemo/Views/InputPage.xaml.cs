using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class InputPage : UserControl
    {
        public InputPage()
        {
            InitializeComponent();
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoInput == null || SizeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoInput.Size = content switch
            {
                "Small" => AntSize.Small,
                "Default" => AntSize.Default,
                "Large" => AntSize.Large,
                _ => AntSize.Default
            };
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoInput != null && sender is CheckBox checkBox)
            {
                DemoInput.IsEnabled = checkBox.IsChecked != true;
            }
        }
    }
}
