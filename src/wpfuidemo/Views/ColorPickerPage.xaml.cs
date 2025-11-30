using System.Windows;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class ColorPickerPage : UserControl
    {
        public ColorPickerPage()
        {
            InitializeComponent();
        }

        private void DisabledCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoColorPicker != null && sender is CheckBox checkBox)
            {
                DemoColorPicker.IsEnabled = checkBox.IsChecked != true;
            }
        }
    }
}
