using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls;

public class PropertyItem : ContentControl
{
    static PropertyItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyItem), new FrameworkPropertyMetadata(typeof(PropertyItem)));
    }

    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
        nameof(Category), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string Category
    {
        get => (string) GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
        nameof(DisplayName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string DisplayName
    {
        get => (string) GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string Description
    {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(PropertyItem), new PropertyMetadata(default(bool)));

    public bool IsReadOnly
    {
        get => (bool) GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(
        nameof(DefaultValue), typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

    public object DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(
        nameof(Editor), typeof(FrameworkElement), typeof(PropertyItem), new PropertyMetadata(default(FrameworkElement), OnEditorChanged));

    private static void OnEditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (PropertyItem) d;
        if (e.NewValue != null)
        {
            ctl.Content = e.NewValue;
        }
    }

    public FrameworkElement Editor
    {
        get => (FrameworkElement) GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
        nameof(PropertyName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string PropertyName
    {
        get => (string) GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
        nameof(PropertyType), typeof(Type), typeof(PropertyItem), new PropertyMetadata(default(Type)));

    public Type PropertyType
    {
        get => (Type) GetValue(PropertyTypeProperty);
        set => SetValue(PropertyTypeProperty, value);
    }

    public static readonly DependencyProperty PropertyTypeNameProperty = DependencyProperty.Register(
        nameof(PropertyTypeName), typeof(string), typeof(PropertyItem), new PropertyMetadata(default(string)));

    public string PropertyTypeName
    {
        get => (string) GetValue(PropertyTypeNameProperty);
        set => SetValue(PropertyTypeNameProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(object), typeof(PropertyItem), new PropertyMetadata(default(object)));

    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public void InitElement()
    {
        // Initialization logic if needed
    }

    public void Show(bool show = true)
    {
        Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }
}
