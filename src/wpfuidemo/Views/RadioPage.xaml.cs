using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class RadioPage : UserControl
    {
        public RadioPage()
        {
            InitializeComponent();
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoRadio1 == null || TypeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            var type = content switch
            {
                "Default" => ButtonType.Default,
                "Primary" => ButtonType.Primary,
                _ => ButtonType.Default
            };

            DemoRadio1.Type = type;
            DemoRadio2.Type = type;
            DemoRadio3.Type = type;
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoRadio1 == null || SizeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            var size = content switch
            {
                "Small" => AntSize.Small,
                "Default" => AntSize.Default,
                "Large" => AntSize.Large,
                _ => AntSize.Default
            };

            DemoRadio1.Size = size;
            DemoRadio2.Size = size;
            DemoRadio3.Size = size;
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoRadio1 != null && sender is CheckBox checkBox)
            {
                bool isEnabled = checkBox.IsChecked != true;
                DemoRadio1.IsEnabled = isEnabled;
                DemoRadio2.IsEnabled = isEnabled;
                DemoRadio3.IsEnabled = isEnabled;
            }
        }
    }
}
