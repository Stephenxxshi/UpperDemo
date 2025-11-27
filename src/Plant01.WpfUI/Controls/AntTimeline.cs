using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public class AntTimeline : ItemsControl
    {
        static AntTimeline()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntTimeline), new FrameworkPropertyMetadata(typeof(AntTimeline)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AntTimelineItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (element is AntTimelineItem timelineItem)
            {
                var index = ItemContainerGenerator.IndexFromContainer(element);
                timelineItem.IsLast = index == Items.Count - 1;
            }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            // Refresh IsLast for all items when collection changes
            // This is a bit expensive but ensures correctness
            for (int i = 0; i < Items.Count; i++)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(i) as AntTimelineItem;
                if (container != null)
                {
                    container.IsLast = i == Items.Count - 1;
                }
            }
        }
    }
}
