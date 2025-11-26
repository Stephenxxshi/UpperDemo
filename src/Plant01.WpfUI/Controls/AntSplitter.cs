using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntSplitter : GridSplitter
    {
        static AntSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntSplitter), new FrameworkPropertyMetadata(typeof(AntSplitter)));
        }
    }
}
