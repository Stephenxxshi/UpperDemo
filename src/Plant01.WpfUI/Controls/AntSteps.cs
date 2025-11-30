using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntSteps : ItemsControl
    {
        static AntSteps()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntSteps), new FrameworkPropertyMetadata(typeof(AntSteps)));
        }

        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(
            nameof(Current), typeof(int), typeof(AntSteps), new PropertyMetadata(0, OnCurrentChanged));

        public int Current
        {
            get => (int)GetValue(CurrentProperty);
            set => SetValue(CurrentProperty, value);
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(AntStepStatus), typeof(AntSteps), new PropertyMetadata(AntStepStatus.Process, OnStatusChanged));

        public AntStepStatus Status
        {
            get => (AntStepStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
            nameof(Direction), typeof(Orientation), typeof(AntSteps), new PropertyMetadata(Orientation.Horizontal));

        public Orientation Direction
        {
            get => (Orientation)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(AntSize), typeof(AntSteps), new PropertyMetadata(AntSize.Default, OnAppearanceChanged));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
            nameof(IsClickable), typeof(bool), typeof(AntSteps), new PropertyMetadata(false));

        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }

        public static readonly DependencyProperty ProgressDotProperty = DependencyProperty.Register(
            nameof(ProgressDot), typeof(bool), typeof(AntSteps), new PropertyMetadata(false, OnAppearanceChanged));

        public bool ProgressDot
        {
            get => (bool)GetValue(ProgressDotProperty);
            set => SetValue(ProgressDotProperty, value);
        }

        public static readonly DependencyProperty LabelPlacementProperty = DependencyProperty.Register(
            nameof(LabelPlacement), typeof(AntStepLabelPlacement), typeof(AntSteps), new PropertyMetadata(AntStepLabelPlacement.Horizontal, OnAppearanceChanged));

        public AntStepLabelPlacement LabelPlacement
        {
            get => (AntStepLabelPlacement)GetValue(LabelPlacementProperty);
            set => SetValue(LabelPlacementProperty, value);
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(AntStepType), typeof(AntSteps), new PropertyMetadata(AntStepType.Default, OnAppearanceChanged));

        public AntStepType Type
        {
            get => (AntStepType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register(
            nameof(StartIndex), typeof(int), typeof(AntSteps), new PropertyMetadata(0, OnAppearanceChanged));

        public int StartIndex
        {
            get => (int)GetValue(StartIndexProperty);
            set => SetValue(StartIndexProperty, value);
        }

        public static readonly DependencyProperty FinishIconProperty = DependencyProperty.Register(
            nameof(FinishIcon), typeof(object), typeof(AntSteps), new PropertyMetadata(null, OnAppearanceChanged));

        public object FinishIcon
        {
            get => GetValue(FinishIconProperty);
            set => SetValue(FinishIconProperty, value);
        }

        public static readonly DependencyProperty ErrorIconProperty = DependencyProperty.Register(
            nameof(ErrorIcon), typeof(object), typeof(AntSteps), new PropertyMetadata(null, OnAppearanceChanged));

        public object ErrorIcon
        {
            get => GetValue(ErrorIconProperty);
            set => SetValue(ErrorIconProperty, value);
        }

        public static readonly RoutedEvent StepChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(StepChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<int>), typeof(AntSteps));

        public event RoutedPropertyChangedEventHandler<int> StepChanged
        {
            add => AddHandler(StepChangedEvent, value);
            remove => RemoveHandler(StepChangedEvent, value);
        }

        private static void OnCurrentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AntSteps)d).UpdateItemsStatus();
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AntSteps)d).UpdateItemsStatus();
        }

        private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AntSteps)d).UpdateItemsAppearance();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AntStepItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AntStepItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (element is AntStepItem stepItem)
            {
                stepItem.Index = ItemContainerGenerator.IndexFromContainer(element);
                stepItem.IsLast = stepItem.Index == Items.Count - 1;
                stepItem.Size = Size; // Propagate Size
                stepItem.ProgressDot = ProgressDot;
                stepItem.LabelPlacement = LabelPlacement;
                stepItem.Type = Type;
                stepItem.DisplayIndex = stepItem.Index + StartIndex;
                stepItem.FinishIcon = FinishIcon;
                stepItem.ErrorIcon = ErrorIcon;

                stepItem.MouseLeftButtonUp -= OnStepItemClick;
                stepItem.MouseLeftButtonUp += OnStepItemClick;

                UpdateItemStatus(stepItem);
            }
        }

        private void OnStepItemClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsClickable && sender is AntStepItem stepItem)
            {
                int oldIndex = Current;
                int newIndex = stepItem.Index;
                if (oldIndex != newIndex)
                {
                    Current = newIndex;
                    RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldIndex, newIndex, StepChangedEvent));
                }
            }
        }

        private void UpdateItemsAppearance()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is AntStepItem item)
                {
                    item.Size = Size;
                    item.ProgressDot = ProgressDot;
                    item.LabelPlacement = LabelPlacement;
                    item.Type = Type;
                    item.DisplayIndex = item.Index + StartIndex;
                    item.FinishIcon = FinishIcon;
                    item.ErrorIcon = ErrorIcon;
                }
            }
        }

        private void UpdateItemsStatus()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is AntStepItem item)
                {
                    UpdateItemStatus(item);
                }
            }
        }

        private void UpdateItemStatus(AntStepItem item)
        {
            if (item.Index < Current)
            {
                item.Status = AntStepStatus.Finish;
            }
            else if (item.Index == Current)
            {
                item.Status = Status;
            }
            else
            {
                item.Status = AntStepStatus.Wait;
            }
        }
    }
}
