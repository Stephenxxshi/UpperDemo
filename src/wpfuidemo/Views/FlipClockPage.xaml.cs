using System.Windows;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class FlipClockPage : UserControl
    {
        private int _count = 0;

        public FlipClockPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _count++;
            if (_count > 9) _count = 0;
            DemoNumber.Number = _count;
        }
    }
}
