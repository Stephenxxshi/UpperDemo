using System.Windows;
using System.Windows.Controls;
using Plant01.WpfUI.Controls;
using Plant01.WpfUI.Helpers;

namespace wpfuidemo.Views
{
    public partial class VirtualKeyboardPage : UserControl
    {
        public VirtualKeyboardPage()
        {
            InitializeComponent();
        }

        private void EnabledCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (DemoInput != null)
                VirtualKeyboardHelper.SetIsEnabled(DemoInput, true);
        }

        private void EnabledCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DemoInput != null)
                VirtualKeyboardHelper.SetIsEnabled(DemoInput, false);
        }

        private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoInput == null) return;

            if (ModeCombo.SelectedIndex == 0)
            {
                VirtualKeyboardHelper.SetKeyboardMode(DemoInput, VirtualKeyboardMode.Full);
            }
            else
            {
                VirtualKeyboardHelper.SetKeyboardMode(DemoInput, VirtualKeyboardMode.Numeric);
            }
        }

        private void HeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DemoInput != null)
            {
                VirtualKeyboardHelper.SetKeyHeight(DemoInput, e.NewValue);
            }
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DemoInput != null)
            {
                VirtualKeyboardHelper.SetKeyFontSize(DemoInput, e.NewValue);
            }
        }

        private void MarginSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DemoInput != null)
            {
                VirtualKeyboardHelper.SetKeyMargin(DemoInput, new Thickness(e.NewValue));
            }
        }
    }
}