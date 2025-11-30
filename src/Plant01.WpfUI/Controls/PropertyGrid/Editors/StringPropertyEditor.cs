using System.Windows;
using System.Windows.Data;

namespace Plant01.WpfUI.Controls;

public class StringPropertyEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        var element = new AntInput();
        BindingOperations.SetBinding(element, AntInput.TextProperty, CreateBinding(propertyItem));
        element.IsEnabled = !propertyItem.IsReadOnly;
        return element;
    }
}
