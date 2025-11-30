using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class ButtonPage : UserControl
    {
        public ButtonPage()
        {
            InitializeComponent();
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoButton == null || TypeCombo.SelectedItem is not ComboBoxItem item) return;
            
            var content = item.Content.ToString();
            DemoButton.Type = content switch
            {
                "Default" => ButtonType.Default,
                "Primary" => ButtonType.Primary,
                "Dashed" => ButtonType.Dashed,
                "Text" => ButtonType.Text,
                "Link" => ButtonType.Link,
                _ => ButtonType.Default
            };
        }

        private void ShapeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoButton == null || ShapeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoButton.Shape = content switch
            {
                "Default" => ButtonShape.Default,
                "Circle" => ButtonShape.Circle,
                "Round" => ButtonShape.Round,
                _ => ButtonShape.Default
            };
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoButton == null || SizeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoButton.Size = content switch
            {
                "Small" => AntSize.Small,
                "Default" => AntSize.Default,
                "Large" => AntSize.Large,
                _ => AntSize.Default
            };
        }

        private void LoadingCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoButton != null) DemoButton.Loading = LoadingCheck.IsChecked == true;
        }

        private void DangerCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoButton != null) DemoButton.Danger = DangerCheck.IsChecked == true;
        }

        private void GhostCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoButton != null) DemoButton.Ghost = GhostCheck.IsChecked == true;
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoButton != null) DemoButton.IsEnabled = DisabledCheck.IsChecked != true;
        }
    }
}
