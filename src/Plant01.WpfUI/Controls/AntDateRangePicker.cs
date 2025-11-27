using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Plant01.WpfUI.Controls
{
    public class AntDateRangePicker : Control
    {
        static AntDateRangePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntDateRangePicker), new FrameworkPropertyMetadata(typeof(AntDateRangePicker)));
        }

        public static readonly DependencyProperty SelectedDateStartProperty = DependencyProperty.Register(
            nameof(SelectedDateStart), typeof(DateTime?), typeof(AntDateRangePicker), new PropertyMetadata(null));

        public DateTime? SelectedDateStart
        {
            get => (DateTime?)GetValue(SelectedDateStartProperty);
            set => SetValue(SelectedDateStartProperty, value);
        }

        public static readonly DependencyProperty SelectedDateEndProperty = DependencyProperty.Register(
            nameof(SelectedDateEnd), typeof(DateTime?), typeof(AntDateRangePicker), new PropertyMetadata(null));

        public DateTime? SelectedDateEnd
        {
            get => (DateTime?)GetValue(SelectedDateEndProperty);
            set => SetValue(SelectedDateEndProperty, value);
        }

        public static readonly DependencyProperty PlaceholderStartProperty = DependencyProperty.Register(
            nameof(PlaceholderStart), typeof(string), typeof(AntDateRangePicker), new PropertyMetadata("开始日期"));

        public string PlaceholderStart
        {
            get => (string)GetValue(PlaceholderStartProperty);
            set => SetValue(PlaceholderStartProperty, value);
        }

        public static readonly DependencyProperty PlaceholderEndProperty = DependencyProperty.Register(
            nameof(PlaceholderEnd), typeof(string), typeof(AntDateRangePicker), new PropertyMetadata("结束日期"));

        public string PlaceholderEnd
        {
            get => (string)GetValue(PlaceholderEndProperty);
            set => SetValue(PlaceholderEndProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(AntDateRangePicker), new PropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_StartPicker") is AntDatePicker startPicker &&
                GetTemplateChild("PART_EndPicker") is AntDatePicker endPicker)
            {
                // Bind DisplayDateStart of EndPicker to SelectedDateStart
                var startBinding = new Binding(nameof(SelectedDateStart))
                {
                    Source = this
                };
                endPicker.SetBinding(DatePicker.DisplayDateStartProperty, startBinding);

                // Bind DisplayDateEnd of StartPicker to SelectedDateEnd
                var endBinding = new Binding(nameof(SelectedDateEnd))
                {
                    Source = this
                };
                startPicker.SetBinding(DatePicker.DisplayDateEndProperty, endBinding);
            }
        }
    }
}
