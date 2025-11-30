using System.Windows;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class CheckboxPage : UserControl
    {
        public CheckboxPage()
        {
            InitializeComponent();
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoCheckbox != null && sender is CheckBox checkBox)
            {
                DemoCheckbox.IsEnabled = checkBox.IsChecked != true;
            }
        }
    }
}
