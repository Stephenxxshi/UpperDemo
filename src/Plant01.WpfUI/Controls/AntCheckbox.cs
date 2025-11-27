using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntCheckbox : CheckBox
    {
        static AntCheckbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntCheckbox), new FrameworkPropertyMetadata(typeof(AntCheckbox)));
        }
    }
}
