using System.Windows;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class CalendarPage : UserControl
    {
        public CalendarPage()
        {
            InitializeComponent();
        }

        private void SelectionModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoCalendar == null || SelectionModeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoCalendar.SelectionMode = content switch
            {
                "SingleDate" => CalendarSelectionMode.SingleDate,
                "SingleRange" => CalendarSelectionMode.SingleRange,
                "MultipleRange" => CalendarSelectionMode.MultipleRange,
                "None" => CalendarSelectionMode.None,
                _ => CalendarSelectionMode.SingleDate
            };
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoCalendar != null && sender is CheckBox checkBox)
            {
                DemoCalendar.IsEnabled = checkBox.IsChecked != true;
            }
        }
    }
}
