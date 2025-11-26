using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public enum DividerOrientation
    {
        Horizontal,
        Vertical
    }

    public enum DividerTextAlignment
    {
        Left,
        Center,
        Right
    }

    public class AntDivider : Control
    {
        static AntDivider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntDivider), new FrameworkPropertyMetadata(typeof(AntDivider)));
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation), typeof(DividerOrientation), typeof(AntDivider), new PropertyMetadata(DividerOrientation.Horizontal));

        public DividerOrientation Orientation
        {
            get => (DividerOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty DashedProperty = DependencyProperty.Register(
            nameof(Dashed), typeof(bool), typeof(AntDivider), new PropertyMetadata(false));

        public bool Dashed
        {
            get => (bool)GetValue(DashedProperty);
            set => SetValue(DashedProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(AntDivider), new PropertyMetadata(null));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            nameof(TextAlignment), typeof(DividerTextAlignment), typeof(AntDivider), new PropertyMetadata(DividerTextAlignment.Center));

        public DividerTextAlignment TextAlignment
        {
            get => (DividerTextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }
    }
}
