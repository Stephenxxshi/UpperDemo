using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntEmpty : ContentControl
    {
        static AntEmpty()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntEmpty), new FrameworkPropertyMetadata(typeof(AntEmpty)));
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(object), typeof(AntEmpty), new PropertyMetadata("No Data"));

        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            nameof(Image), typeof(object), typeof(AntEmpty), new PropertyMetadata(null));

        public object Image
        {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
    }
}
