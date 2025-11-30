using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls;

public class DescriptionsPanel : Panel
{
    public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
        nameof(Column), typeof(int), typeof(DescriptionsPanel), new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public int Column
    {
        get => (int) GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var columnCount = Math.Max(1, Column);
        var itemWidth = availableSize.Width / columnCount;
        var children = InternalChildren.OfType<UIElement>().ToList();

        double currentRowHeight = 0;
        double totalHeight = 0;
        int currentColumn = 0;

        foreach (var child in children)
        {
            int span = 1;
            if (child is AntDescriptionsItem item)
            {
                span = Math.Min(item.Span, columnCount); // Span cannot exceed total columns
            }
            else if (child is FrameworkElement fe && fe.DataContext is AntDescriptionsItem dataItem)
            {
                 // Handle case where container is generated
                 span = Math.Min(dataItem.Span, columnCount);
            }
            
            // If current item exceeds remaining columns, move to next row
            if (currentColumn + span > columnCount)
            {
                totalHeight += currentRowHeight;
                currentRowHeight = 0;
                currentColumn = 0;
            }

            var childAvailableWidth = itemWidth * span;
            child.Measure(new Size(childAvailableWidth, double.PositiveInfinity));
            
            currentRowHeight = Math.Max(currentRowHeight, child.DesiredSize.Height);
            currentColumn += span;

            if (currentColumn >= columnCount)
            {
                totalHeight += currentRowHeight;
                currentRowHeight = 0;
                currentColumn = 0;
            }
        }

        totalHeight += currentRowHeight;

        return new Size(availableSize.Width, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columnCount = Math.Max(1, Column);
        var itemWidth = finalSize.Width / columnCount;
        var children = InternalChildren.OfType<UIElement>().ToList();

        double y = 0;
        double currentRowHeight = 0;
        int currentColumn = 0;
        
        // First pass to calculate row heights
        // This is a simplified approach. For perfect grid alignment with varying heights in same row, 
        // we need to know which items are in which row.
        
        // Let's group items by row first
        var rows = new System.Collections.Generic.List<System.Collections.Generic.List<(UIElement Element, int Span)>>();
        var currentRow = new System.Collections.Generic.List<(UIElement Element, int Span)>();
        
        int tempCol = 0;
        foreach (var child in children)
        {
            int span = 1;
            if (child is AntDescriptionsItem item) span = item.Span;
            // Try to get span from attached property or container if needed, for now assume 1 if not AntDescriptionsItem
            
            span = Math.Min(span, columnCount);

            if (tempCol + span > columnCount)
            {
                rows.Add(currentRow);
                currentRow = new System.Collections.Generic.List<(UIElement Element, int Span)>();
                tempCol = 0;
            }
            
            currentRow.Add((child, span));
            tempCol += span;
        }
        if (currentRow.Count > 0) rows.Add(currentRow);

        // Now arrange
        foreach (var row in rows)
        {
            double rowHeight = 0;
            foreach (var (element, span) in row)
            {
                rowHeight = Math.Max(rowHeight, element.DesiredSize.Height);
            }

            double x = 0;
            foreach (var (element, span) in row)
            {
                var width = itemWidth * span;
                element.Arrange(new Rect(x, y, width, rowHeight));
                x += width;
            }
            y += rowHeight;
        }

        return finalSize;
    }
}
