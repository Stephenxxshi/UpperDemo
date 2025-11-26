using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntMenu : TreeView
    {
        static AntMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntMenu), new FrameworkPropertyMetadata(typeof(AntMenu)));
        }
    }
}
