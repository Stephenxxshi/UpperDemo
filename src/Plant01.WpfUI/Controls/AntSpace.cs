using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntSpace : ItemsControl
    {
        static AntSpace()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntSpace), new FrameworkPropertyMetadata(typeof(AntSpace)));
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            nameof(Direction), typeof(Orientation), typeof(AntSpace), new PropertyMetadata(Orientation.Horizontal));

        public Orientation Direction
        {
            get => (Orientation)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(double), typeof(AntSpace), new PropertyMetadata(8.0));

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
    }
}
