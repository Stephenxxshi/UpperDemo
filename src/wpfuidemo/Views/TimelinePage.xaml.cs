using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class TimelinePage : UserControl
    {
        public TimelinePage()
        {
            InitializeComponent();
        }

        private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoTimeline == null || ModeCombo.SelectedItem is not ComboBoxItem item) return;

            // Mode property is not yet implemented in AntTimeline
            /*
            var content = item.Content.ToString();
            DemoTimeline.Mode = content switch
            {
                "Left" => TimelineMode.Left,
                "Right" => TimelineMode.Right,
                "Alternate" => TimelineMode.Alternate,
                _ => TimelineMode.Left
            };
            */
        }
    }
}
