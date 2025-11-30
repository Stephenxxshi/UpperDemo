using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class DatePickerPage : UserControl
    {
        public DatePickerPage()
        {
            InitializeComponent();
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoDatePicker == null || SizeCombo.SelectedItem is not ComboBoxItem item) return;

            // Size property is not yet implemented in AntDatePicker / AntDateRangePicker
            /*
            var content = item.Content.ToString();
            var size = content switch
            {
                "Small" => AntSize.Small,
                "Default" => AntSize.Default,
                "Large" => AntSize.Large,
                _ => AntSize.Default
            };

            DemoDatePicker.Size = size;
            DemoRangePicker.Size = size;
            */
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoDatePicker != null && sender is CheckBox checkBox)
            {
                bool isEnabled = checkBox.IsChecked != true;
                DemoDatePicker.IsEnabled = isEnabled;
                DemoRangePicker.IsEnabled = isEnabled;
            }
        }
    }
}
