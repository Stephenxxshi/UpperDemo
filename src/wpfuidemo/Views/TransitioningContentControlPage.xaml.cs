using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Plant01.WpfUI.Controls;
using Plant01.WpfUI.Themes;

namespace wpfuidemo.Views
{
    public partial class TransitioningContentControlPage : UserControl
    {
        private bool _isContent1 = true;

        public TransitioningContentControlPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _isContent1 = !_isContent1;
            if (_isContent1)
            {
                var border = new Border
                {
                    Background = (Brush)FindResource(DesignTokenKeys.PrimaryColor),
                    Width = 200,
                    Height = 200,
                    CornerRadius = new CornerRadius(10)
                };
                var text = new TextBlock
                {
                    Text = "Content 1",
                    Foreground = Brushes.White,
                    FontSize = 24,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                border.Child = text;
                TransitionControl.Content = border;
            }
            else
            {
                var border = new Border
                {
                    Background = (Brush)FindResource(DesignTokenKeys.SuccessColor),
                    Width = 200,
                    Height = 200,
                    CornerRadius = new CornerRadius(10)
                };
                var text = new TextBlock
                {
                    Text = "Content 2",
                    Foreground = Brushes.White,
                    FontSize = 24,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                border.Child = text;
                TransitionControl.Content = border;
            }
        }

        private void TransitionModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransitionControl != null && TransitionModeComboBox.SelectedItem is ComboBoxItem item)
            {
                if (System.Enum.TryParse<TransitionMode>(item.Content.ToString(), out var mode))
                {
                    TransitionControl.TransitionMode = mode;
                }
            }
        }
    }
}
