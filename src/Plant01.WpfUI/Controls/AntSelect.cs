using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plant01.WpfUI.Controls
{
    public class AntSelect : ComboBox
    {
        static AntSelect()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntSelect), new FrameworkPropertyMetadata(typeof(AntSelect)));
        }

        public AntSelect()
        {
            ClearSelectionCommand = new SimpleCommand(OnClearSelection);
        }

        private void OnClearSelection(object? obj)
        {
            SetCurrentValue(SelectedIndexProperty, -1);
            SetCurrentValue(SelectedItemProperty, null);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(AntSelect), new PropertyMetadata(new CornerRadius(2)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(AntSelect), new PropertyMetadata("请选择"));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public static readonly DependencyProperty AllowClearProperty = DependencyProperty.Register(
            nameof(AllowClear), typeof(bool), typeof(AntSelect), new PropertyMetadata(false));

        public bool AllowClear
        {
            get => (bool)GetValue(AllowClearProperty);
            set => SetValue(AllowClearProperty, value);
        }

        public static readonly DependencyProperty ClearSelectionCommandProperty = DependencyProperty.Register(
            nameof(ClearSelectionCommand), typeof(ICommand), typeof(AntSelect), new PropertyMetadata(null));

        public ICommand ClearSelectionCommand
        {
            get => (ICommand)GetValue(ClearSelectionCommandProperty);
            private set => SetValue(ClearSelectionCommandProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(AntSize), typeof(AntSelect), new PropertyMetadata(AntSize.Default));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
    }
}
