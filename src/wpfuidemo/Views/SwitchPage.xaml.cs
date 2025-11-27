using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class SwitchPage : UserControl
    {
        public SwitchPage()
        {
            InitializeComponent();
        }

        private void Placement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSwitch == null) return;
            
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                var content = item.Content.ToString();
                switch (content)
                {
                    case "Inside":
                        DemoSwitch.TextPlacement = SwitchTextPlacement.Inside;
                        break;
                    case "Left":
                        DemoSwitch.TextPlacement = SwitchTextPlacement.Left;
                        break;
                    case "Right":
                        DemoSwitch.TextPlacement = SwitchTextPlacement.Right;
                        break;
                }
            }
        }
    }
}
