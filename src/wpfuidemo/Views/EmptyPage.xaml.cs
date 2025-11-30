using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class EmptyPage : UserControl
    {
        public EmptyPage()
        {
            InitializeComponent();
        }

        private void FooterCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (DemoEmpty == null) return;

            if (FooterCheck.IsChecked == true)
            {
                var btn = new AntButton { Content = "Create Now", Type = ButtonType.Primary };
                DemoEmpty.Content = btn;
            }
            else
            {
                DemoEmpty.Content = null;
            }
        }
    }
}
