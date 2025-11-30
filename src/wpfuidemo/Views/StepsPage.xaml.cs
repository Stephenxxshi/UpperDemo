using System.Windows.Controls;
using Plant01.WpfUI.Controls;

namespace wpfuidemo.Views
{
    public partial class StepsPage : UserControl
    {
        public StepsPage()
        {
            InitializeComponent();
        }

        private void StatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSteps == null || StatusCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoSteps.Status = content switch
            {
                "Process" => AntStepStatus.Process,
                "Wait" => AntStepStatus.Wait,
                "Finish" => AntStepStatus.Finish,
                "Error" => AntStepStatus.Error,
                _ => AntStepStatus.Process
            };
        }

        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSteps == null || SizeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoSteps.Size = content switch
            {
                "Small" => AntSize.Small,
                "Default" => AntSize.Default,
                "Large" => AntSize.Large,
                _ => AntSize.Default
            };
        }

        private void DirectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSteps == null || DirectionCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoSteps.Direction = content switch
            {
                "Horizontal" => Orientation.Horizontal,
                "Vertical" => Orientation.Vertical,
                _ => Orientation.Horizontal
            };
        }

        private void LabelPlacementCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSteps == null || LabelPlacementCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoSteps.LabelPlacement = content switch
            {
                "Horizontal" => AntStepLabelPlacement.Horizontal,
                "Vertical" => AntStepLabelPlacement.Vertical,
                _ => AntStepLabelPlacement.Horizontal
            };
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemoSteps == null || TypeCombo.SelectedItem is not ComboBoxItem item) return;

            var content = item.Content.ToString();
            DemoSteps.Type = content switch
            {
                "Default" => AntStepType.Default,
                "Navigation" => AntStepType.Navigation,
                _ => AntStepType.Default
            };
        }
    }
}
