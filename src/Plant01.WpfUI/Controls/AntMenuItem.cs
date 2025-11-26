using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Plant01.WpfUI.Controls
{
    public class AntMenuItem : TreeViewItem
    {
        static AntMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntMenuItem), new FrameworkPropertyMetadata(typeof(AntMenuItem)));
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(AntMenuItem), new PropertyMetadata(null));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconFontFamilyProperty =
            DependencyProperty.Register(nameof(IconFontFamily), typeof(FontFamily), typeof(AntMenuItem), new PropertyMetadata(new FontFamily("Segoe UI Emoji")));

        public FontFamily IconFontFamily
        {
            get => (FontFamily)GetValue(IconFontFamilyProperty);
            set => SetValue(IconFontFamilyProperty, value);
        }
        
        // Helper to check if it's a root item (for indentation logic if needed, though TreeView handles this via Margin usually)
        public int Level
        {
            get
            {
                int level = 0;
                DependencyObject parent = ItemsControl.ItemsControlFromItemContainer(this);
                while (parent != null && parent is AntMenuItem)
                {
                    level++;
                    parent = ItemsControl.ItemsControlFromItemContainer((DependencyObject)parent);
                }
                return level;
            }
        }
    }
}
