using System;
using System.ComponentModel;
using System.Windows;
using Plant01.WpfUI.Controls.PropertyGrids.Editors;

namespace Plant01.WpfUI.Controls.PropertyGrids;

public class PropertyResolver
{
    public virtual string ResolveCategory(PropertyDescriptor propertyDescriptor)
    {
        var attribute = propertyDescriptor.Attributes[typeof(CategoryAttribute)] as CategoryAttribute;
        return attribute?.Category ?? "Misc";
    }

    public virtual string ResolveDisplayName(PropertyDescriptor propertyDescriptor)
    {
        var attribute = propertyDescriptor.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
        return attribute?.DisplayName ?? propertyDescriptor.Name;
    }

    public virtual string ResolveDescription(PropertyDescriptor propertyDescriptor)
    {
        var attribute = propertyDescriptor.Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
        return attribute?.Description;
    }

    public virtual bool ResolveIsReadOnly(PropertyDescriptor propertyDescriptor)
    {
        var attribute = propertyDescriptor.Attributes[typeof(ReadOnlyAttribute)] as ReadOnlyAttribute;
        return attribute?.IsReadOnly ?? propertyDescriptor.IsReadOnly;
    }

    public virtual object ResolveDefaultValue(PropertyDescriptor propertyDescriptor)
    {
        var attribute = propertyDescriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
        return attribute?.Value;
    }

    public virtual bool ResolveIsBrowsable(PropertyDescriptor propertyDescriptor)
    {
        var attribute = propertyDescriptor.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
        return attribute?.Browsable ?? true;
    }

    public virtual FrameworkElement ResolveEditor(PropertyDescriptor propertyDescriptor)
    {
        var isReadOnly = ResolveIsReadOnly(propertyDescriptor);
        if (isReadOnly)
        {
            return new ReadOnlyPropertyEditor().CreateElement(new PropertyItem
            {
                PropertyName = propertyDescriptor.Name,
                IsReadOnly = true
            });
        }

        var type = propertyDescriptor.PropertyType;
        PropertyEditorBase editor = null;

        if (type == typeof(string))
        {
            editor = new StringPropertyEditor();
        }
        else if (type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal))
        {
            // For now use StringPropertyEditor, ideally NumberPropertyEditor
            editor = new StringPropertyEditor();
        }
        else if (type == typeof(bool))
        {
            // editor = new SwitchPropertyEditor(); // Need to implement
            editor = new StringPropertyEditor(); // Fallback
        }
        else if (type.IsEnum)
        {
            // editor = new EnumPropertyEditor(); // Need to implement
            editor = new StringPropertyEditor(); // Fallback
        }
        else
        {
            editor = new ReadOnlyPropertyEditor();
        }

        if (editor != null)
        {
            return editor.CreateElement(new PropertyItem
            {
                PropertyName = propertyDescriptor.Name,
                PropertyType = type,
                IsReadOnly = isReadOnly
            });
        }

        return null;
    }
}
