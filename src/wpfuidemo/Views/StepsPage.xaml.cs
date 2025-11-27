using System.Windows;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class StepsPage : UserControl
    {
        public StepsPage()
        {
            InitializeComponent();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (InteractiveSteps.Current < InteractiveSteps.Items.Count - 1)
            {
                InteractiveSteps.Current++;
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (InteractiveSteps.Current > 0)
            {
                InteractiveSteps.Current--;
            }
        }
    }
}
