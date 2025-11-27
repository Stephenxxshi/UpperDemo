using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntDatePicker : DatePicker
    {
        static AntDatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntDatePicker), new FrameworkPropertyMetadata(typeof(AntDatePicker)));
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(AntDatePicker), new PropertyMetadata("请选择日期"));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(AntDatePicker), new PropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_ClearButton") is Button clearButton)
            {
                clearButton.Click += OnClearButtonClick;
            }
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(SelectedDateProperty, null);
        }
    }
}
