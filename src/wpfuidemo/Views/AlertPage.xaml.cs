using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class AlertPage : UserControl
    {
        public AlertPage()
        {
            InitializeComponent();
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoAlert == null || TypeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoAlert.Type = content switch
            {
                "Success" => AlertType.Success,
                "Info" => AlertType.Info,
                "Warning" => AlertType.Warning,
                "Error" => AlertType.Error,
                _ => AlertType.Success
            };
        }
    }
}
