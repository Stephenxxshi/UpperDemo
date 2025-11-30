using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Plant01.WpfUI.Controls;

public class ReadOnlyPropertyEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        var element = new TextBlock();
        BindingOperations.SetBinding(element, TextBlock.TextProperty, CreateBinding(propertyItem));
        element.VerticalAlignment = VerticalAlignment.Center;
        return element;
    }
}
