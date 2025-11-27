using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Plant01.WpfUI.Helpers;

namespace Plant01.WpfUI.Controls
{
    public class AntRadioButton : RadioButton
    {
        static AntRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntRadioButton), new FrameworkPropertyMetadata(typeof(AntRadioButton)));
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(ButtonType), typeof(AntRadioButton), new PropertyMetadata(ButtonType.Default));

        public ButtonType Type
        {
            get => (ButtonType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            nameof(Shape), typeof(ButtonShape), typeof(AntRadioButton), new PropertyMetadata(ButtonShape.Default));

        public ButtonShape Shape
        {
            get => (ButtonShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(string), typeof(AntRadioButton), new PropertyMetadata(null, null, IconHelper.CoerceIcon));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        
        public static readonly DependencyProperty IconFontFamilyProperty = DependencyProperty.Register(
            nameof(IconFontFamily), typeof(FontFamily), typeof(AntRadioButton), new PropertyMetadata(new FontFamily("Segoe UI Symbol")));

        public FontFamily IconFontFamily
        {
            get => (FontFamily)GetValue(IconFontFamilyProperty);
            set => SetValue(IconFontFamilyProperty, value);
        }

        public static readonly DependencyProperty LoadingProperty = DependencyProperty.Register(
            nameof(Loading), typeof(bool), typeof(AntRadioButton), new PropertyMetadata(false));

        public bool Loading
        {
            get => (bool)GetValue(LoadingProperty);
            set => SetValue(LoadingProperty, value);
        }

        public static readonly DependencyProperty DangerProperty = DependencyProperty.Register(
            nameof(Danger), typeof(bool), typeof(AntRadioButton), new PropertyMetadata(false));

        public bool Danger
        {
            get => (bool)GetValue(DangerProperty);
            set => SetValue(DangerProperty, value);
        }

        public static readonly DependencyProperty GhostProperty = DependencyProperty.Register(
            nameof(Ghost), typeof(bool), typeof(AntRadioButton), new PropertyMetadata(false));

        public bool Ghost
        {
            get => (bool)GetValue(GhostProperty);
            set => SetValue(GhostProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(AntRadioButton), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(AntSize), typeof(AntRadioButton), new PropertyMetadata(AntSize.Default));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
    }
}
