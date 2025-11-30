using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls;

public class AntDescriptionsItem : ContentControl
{
    static AntDescriptionsItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AntDescriptionsItem), new FrameworkPropertyMetadata(typeof(AntDescriptionsItem)));
    }

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(object), typeof(AntDescriptionsItem), new PropertyMetadata(null));

    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty SpanProperty = DependencyProperty.Register(
        nameof(Span), typeof(int), typeof(AntDescriptionsItem), new PropertyMetadata(1));

    public int Span
    {
        get => (int) GetValue(SpanProperty);
        set => SetValue(SpanProperty, value);
    }
}
