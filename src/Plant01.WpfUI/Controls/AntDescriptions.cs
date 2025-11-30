using System;
using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls;

public class AntDescriptions : ItemsControl
{
    static AntDescriptions()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AntDescriptions), new FrameworkPropertyMetadata(typeof(AntDescriptions)));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(AntDescriptions), new PropertyMetadata(null));

    public string Title
    {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty BorderedProperty = DependencyProperty.Register(
        nameof(Bordered), typeof(bool), typeof(AntDescriptions), new PropertyMetadata(false));

    public bool Bordered
    {
        get => (bool) GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
        nameof(Column), typeof(int), typeof(AntDescriptions), new PropertyMetadata(3));

    public int Column
    {
        get => (int) GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(AntSize), typeof(AntDescriptions), new PropertyMetadata(AntSize.Default));

    public AntSize Size
    {
        get => (AntSize) GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(Layout), typeof(Orientation), typeof(AntDescriptions), new PropertyMetadata(Orientation.Horizontal));

    public Orientation Layout
    {
        get => (Orientation) GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public static readonly DependencyProperty LabelHorizontalAlignmentProperty = DependencyProperty.Register(
        nameof(LabelHorizontalAlignment), typeof(HorizontalAlignment), typeof(AntDescriptions), new PropertyMetadata(HorizontalAlignment.Left));

    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => (HorizontalAlignment) GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
        nameof(ItemMargin), typeof(Thickness), typeof(AntDescriptions), new PropertyMetadata(new Thickness(0)));

    public Thickness ItemMargin
    {
        get => (Thickness) GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
        nameof(LabelWidth), typeof(GridLength), typeof(AntDescriptions), new PropertyMetadata(GridLength.Auto));

    public GridLength LabelWidth
    {
        get => (GridLength) GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is AntDescriptionsItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new AntDescriptionsItem();
    }
}
