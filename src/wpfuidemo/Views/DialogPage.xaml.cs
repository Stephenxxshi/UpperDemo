using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class DialogPage : UserControl
    {
        public DialogPage()
        {
            InitializeComponent();
        }

        private void OpenModal_Click(object sender, RoutedEventArgs e)
        {
            var modal = new AntModal
            {
                Title = "Basic Modal (Blur)",
                Content = new TextBlock { Text = "This modal uses the default Blur mask.", Margin = new Thickness(0, 10, 0, 0) },
                Width = 400,
                Height = 200,
                Mask = AntModalMask.Blur
            };
            modal.ShowDialog();
        }

        private void OpenDimModal_Click(object sender, RoutedEventArgs e)
        {
            var modal = new AntModal
            {
                Title = "Basic Modal (Dim)",
                Content = new TextBlock { Text = "This modal uses a Dim (Black) mask.", Margin = new Thickness(0, 10, 0, 0) },
                Width = 400,
                Height = 200,
                Mask = AntModalMask.Dim
            };
            modal.ShowDialog();
        }

        private void OpenNoneModal_Click(object sender, RoutedEventArgs e)
        {
            var modal = new AntModal
            {
                Title = "Basic Modal (None)",
                Content = new TextBlock { Text = "This modal has no mask.", Margin = new Thickness(0, 10, 0, 0) },
                Width = 400,
                Height = 200,
                Mask = AntModalMask.None
            };
            modal.ShowDialog();
        }

        private void OpenCustomModal_Click(object sender, RoutedEventArgs e)
        {
            var modal = new AntModal
            {
                Title = "Custom Modal",
                Content = "This is a custom modal dialog.",
                OkText = "Confirm",
                CancelText = "Close"
            };
            modal.ShowDialog();
        }
    }
}
