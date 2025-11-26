using System.Windows;
using System.Windows.Controls.Primitives;

namespace Plant01.WpfUI.Controls
{
    public enum SwitchTextPlacement
    {
        Inside,
        Left,
        Right
    }

    public class AntSwitch : ToggleButton
    {
        static AntSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntSwitch), new FrameworkPropertyMetadata(typeof(AntSwitch)));
        }

        public static readonly DependencyProperty LoadingProperty = DependencyProperty.Register(
            nameof(Loading), typeof(bool), typeof(AntSwitch), new PropertyMetadata(false));

        public bool Loading
        {
            get => (bool)GetValue(LoadingProperty);
            set => SetValue(LoadingProperty, value);
        }

        public static readonly DependencyProperty CheckedContentProperty = DependencyProperty.Register(
            nameof(CheckedContent), typeof(object), typeof(AntSwitch), new PropertyMetadata(null));

        public object CheckedContent
        {
            get => GetValue(CheckedContentProperty);
            set => SetValue(CheckedContentProperty, value);
        }

        public static readonly DependencyProperty UnCheckedContentProperty = DependencyProperty.Register(
            nameof(UnCheckedContent), typeof(object), typeof(AntSwitch), new PropertyMetadata(null));

        public object UnCheckedContent
        {
            get => GetValue(UnCheckedContentProperty);
            set => SetValue(UnCheckedContentProperty, value);
        }

        public static readonly DependencyProperty TextPlacementProperty = DependencyProperty.Register(
            nameof(TextPlacement), typeof(SwitchTextPlacement), typeof(AntSwitch), new PropertyMetadata(SwitchTextPlacement.Inside));

        public SwitchTextPlacement TextPlacement
        {
            get => (SwitchTextPlacement)GetValue(TextPlacementProperty);
            set => SetValue(TextPlacementProperty, value);
        }
    }
}
