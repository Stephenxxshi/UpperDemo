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
            nameof(Size), typeof(AntSize), typeof(AntSteps), new PropertyMetadata(AntSize.Default));

        public AntSize Size
        {
            get => (AntSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        private static void OnCurrentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AntSteps)d).UpdateItemsStatus();
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AntSteps)d).UpdateItemsStatus();
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
                UpdateItemStatus(stepItem);
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
