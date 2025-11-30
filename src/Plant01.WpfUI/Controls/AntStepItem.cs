using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntStepItem : ContentControl
    {
        static AntStepItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntStepItem), new FrameworkPropertyMetadata(typeof(AntStepItem)));
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(AntStepItem), new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(AntStepItem), new PropertyMetadata(null));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(object), typeof(AntStepItem), new PropertyMetadata(null));

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(AntStepStatus), typeof(AntStepItem), new PropertyMetadata(AntStepStatus.Wait));

        public AntStepStatus Status
        {
            get => (AntStepStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
            nameof(Index), typeof(int), typeof(AntStepItem), new PropertyMetadata(0));

        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public static readonly DependencyProperty IsLastProperty = DependencyProperty.Register(
            nameof(IsLast), typeof(bool), typeof(AntStepItem), new PropertyMetadata(false));

        public bool IsLast
        {
            get => (bool)GetValue(IsLastProperty);
            set => SetValue(IsLastProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(AntSize), typeof(AntStepItem), new PropertyMetadata(AntSize.Default));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly DependencyProperty ProgressDotProperty = DependencyProperty.Register(
            nameof(ProgressDot), typeof(bool), typeof(AntStepItem), new PropertyMetadata(false));

        public bool ProgressDot
        {
            get => (bool)GetValue(ProgressDotProperty);
            set => SetValue(ProgressDotProperty, value);
        }

        public static readonly DependencyProperty LabelPlacementProperty = DependencyProperty.Register(
            nameof(LabelPlacement), typeof(AntStepLabelPlacement), typeof(AntStepItem), new PropertyMetadata(AntStepLabelPlacement.Horizontal));

        public AntStepLabelPlacement LabelPlacement
        {
            get => (AntStepLabelPlacement)GetValue(LabelPlacementProperty);
            set => SetValue(LabelPlacementProperty, value);
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(AntStepType), typeof(AntStepItem), new PropertyMetadata(AntStepType.Default));

        public AntStepType Type
        {
            get => (AntStepType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty DisplayIndexProperty = DependencyProperty.Register(
            nameof(DisplayIndex), typeof(int), typeof(AntStepItem), new PropertyMetadata(1));

        public int DisplayIndex
        {
            get => (int)GetValue(DisplayIndexProperty);
            set => SetValue(DisplayIndexProperty, value);
        }

        public static readonly DependencyProperty FinishIconProperty = DependencyProperty.Register(
            nameof(FinishIcon), typeof(object), typeof(AntStepItem), new PropertyMetadata(null));

        public object FinishIcon
        {
            get => GetValue(FinishIconProperty);
            set => SetValue(FinishIconProperty, value);
        }

        public static readonly DependencyProperty ErrorIconProperty = DependencyProperty.Register(
            nameof(ErrorIcon), typeof(object), typeof(AntStepItem), new PropertyMetadata(null));

        public object ErrorIcon
        {
            get => GetValue(ErrorIconProperty);
            set => SetValue(ErrorIconProperty, value);
        }
    }
}
