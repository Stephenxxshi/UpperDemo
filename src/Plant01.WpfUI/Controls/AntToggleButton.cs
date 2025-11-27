using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Plant01.WpfUI.Helpers;

namespace Plant01.WpfUI.Controls
{
    public class AntToggleButton : ToggleButton
    {
        static AntToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntToggleButton), new FrameworkPropertyMetadata(typeof(AntToggleButton)));
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(ButtonType), typeof(AntToggleButton), new PropertyMetadata(ButtonType.Default));

        public ButtonType Type
        {
            get => (ButtonType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
            nameof(Shape), typeof(ButtonShape), typeof(AntToggleButton), new PropertyMetadata(ButtonShape.Default));

        public ButtonShape Shape
        {
            get => (ButtonShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(string), typeof(AntToggleButton), new PropertyMetadata(null, null, IconHelper.CoerceIcon));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        
        public static readonly DependencyProperty IconFontFamilyProperty = DependencyProperty.Register(
            nameof(IconFontFamily), typeof(FontFamily), typeof(AntToggleButton), new PropertyMetadata(new FontFamily("Segoe UI Symbol")));

        public FontFamily IconFontFamily
        {
            get => (FontFamily)GetValue(IconFontFamilyProperty);
            set => SetValue(IconFontFamilyProperty, value);
        }

        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register(
            nameof(IconPlacement), typeof(IconPlacement), typeof(AntToggleButton), new PropertyMetadata(IconPlacement.Left));

        public IconPlacement IconPlacement
        {
            get => (IconPlacement)GetValue(IconPlacementProperty);
            set => SetValue(IconPlacementProperty, value);
        }

        public static readonly DependencyProperty IconFontSizeProperty = DependencyProperty.Register(
            nameof(IconFontSize), typeof(double), typeof(AntToggleButton), new PropertyMetadata(double.NaN));

        public double IconFontSize
        {
            get => (double)GetValue(IconFontSizeProperty);
            set => SetValue(IconFontSizeProperty, value);
        }

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
            nameof(IconWidth), typeof(double), typeof(AntToggleButton), new PropertyMetadata(double.NaN));

        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
            nameof(IconHeight), typeof(double), typeof(AntToggleButton), new PropertyMetadata(double.NaN));

        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
        }

        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(
            nameof(ContentWidth), typeof(double), typeof(AntToggleButton), new PropertyMetadata(double.NaN));

        public double ContentWidth
        {
            get => (double)GetValue(ContentWidthProperty);
            set => SetValue(ContentWidthProperty, value);
        }

        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(
            nameof(ContentHeight), typeof(double), typeof(AntToggleButton), new PropertyMetadata(double.NaN));

        public double ContentHeight
        {
            get => (double)GetValue(ContentHeightProperty);
            set => SetValue(ContentHeightProperty, value);
        }

        public static readonly DependencyProperty ContentFontSizeProperty = DependencyProperty.Register(
            nameof(ContentFontSize), typeof(double), typeof(AntToggleButton), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ContentFontSize
        {
            get => (double)GetValue(ContentFontSizeProperty);
            set => SetValue(ContentFontSizeProperty, value);
        }

        public static readonly DependencyProperty LoadingProperty = DependencyProperty.Register(
            nameof(Loading), typeof(bool), typeof(AntToggleButton), new PropertyMetadata(false));

        public bool Loading
        {
            get => (bool)GetValue(LoadingProperty);
            set => SetValue(LoadingProperty, value);
        }

        public static readonly DependencyProperty DangerProperty = DependencyProperty.Register(
            nameof(Danger), typeof(bool), typeof(AntToggleButton), new PropertyMetadata(false));

        public bool Danger
        {
            get => (bool)GetValue(DangerProperty);
            set => SetValue(DangerProperty, value);
        }

        public static readonly DependencyProperty GhostProperty = DependencyProperty.Register(
            nameof(Ghost), typeof(bool), typeof(AntToggleButton), new PropertyMetadata(false));

        public bool Ghost
        {
            get => (bool)GetValue(GhostProperty);
            set => SetValue(GhostProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(AntToggleButton), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(AntSize), typeof(AntToggleButton), new PropertyMetadata(AntSize.Default));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
    }
}
