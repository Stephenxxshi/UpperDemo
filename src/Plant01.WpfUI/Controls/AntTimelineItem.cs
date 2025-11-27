using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Plant01.WpfUI.Controls
{
    public class AntTimelineItem : ContentControl
    {
        static AntTimelineItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntTimelineItem), new FrameworkPropertyMetadata(typeof(AntTimelineItem)));
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color), typeof(Brush), typeof(AntTimelineItem), new PropertyMetadata(Brushes.Blue));

        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty DotProperty = DependencyProperty.Register(
            nameof(Dot), typeof(object), typeof(AntTimelineItem), new PropertyMetadata(null));

        public object Dot
        {
            get => GetValue(DotProperty);
            set => SetValue(DotProperty, value);
        }
        
        public static readonly DependencyProperty IsLastProperty = DependencyProperty.Register(
            nameof(IsLast), typeof(bool), typeof(AntTimelineItem), new PropertyMetadata(false));

        public bool IsLast
        {
            get => (bool)GetValue(IsLastProperty);
            set => SetValue(IsLastProperty, value);
        }
    }
}
