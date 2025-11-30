using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Plant01.WpfUI.Controls;

public abstract class PropertyEditorBase : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(PropertyEditorBase), new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public virtual FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        return null;
    }

    public virtual DependencyProperty GetDependencyProperty() => ValueProperty;

    protected Binding CreateBinding(PropertyItem propertyItem) => new(propertyItem.PropertyName)
    {
        Source = propertyItem.Value,
        Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        ValidatesOnDataErrors = true,
        ValidatesOnExceptions = true
    };
}
