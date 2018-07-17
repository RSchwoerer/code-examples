using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using DemoAnimatedGroup.Helpers;

namespace DemoAnimatedGroup.Behaviors
{
    public class OverlayGroupingBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.LayoutUpdated += new EventHandler(this.OnLayoutUpdated);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            ListBoxItem topListBoxItem1;
            GroupItem topGroupItem1, topGroupItem2;
            ContentPresenter topPresenter1, topPresenter2 = null;
            double topOffset1, topOffset2 = -1;

            // find the first ListBoxItem which is at the top of the control
            topListBoxItem1 = this.GetItemAtMinimumYOffset<ListBoxItem>();
            if (topListBoxItem1 == null)
                return;

            // get all group items order by their distance to the top
            var groupItems = TreeHelper.FindVisualChildren<GroupItem>(this.AssociatedObject)
                                       .OrderBy(this.GetYOffset)
                                       .ToList();

            // from the GroupItem, find the ContentPresenter on which we can apply the transform
            topGroupItem1 = TreeHelper.FindVisualAncestor<GroupItem>(topListBoxItem1);

            //topPresenter1 = TreeHelper.FindVisualChild<ContentPresenter>(topGroupItem1);
            topPresenter1 = TreeHelper.FindChild<ContentPresenter>(topGroupItem1, "PART_Header");

            topOffset1 = this.GetYOffset(topPresenter1);

            // try to find the next GroupItem and its presenter
            var index = groupItems.IndexOf(topGroupItem1);
            if (index + 1 < groupItems.Count)
            {
                topGroupItem2 = groupItems.ElementAt(index + 1);
                topPresenter2 = TreeHelper.FindVisualChild<ContentPresenter>(topGroupItem2);
                topOffset2 = this.GetYOffset(topPresenter2);
            }

            // update transforms
            if (topOffset2 < 0 || topOffset2 > topPresenter1.ActualHeight)
                this.SetGroupItemOffset(topPresenter1, topOffset1);

            if (topPresenter2 != null)
                topPresenter2.RenderTransform = null;
        }

        private T GetItemAtMinimumYOffset<T>() where T : UIElement
        {
            var minOffset = double.MaxValue;
            T topItem = null;
            foreach (var item in TreeHelper.FindVisualChildren<T>(this.AssociatedObject))
            {
                var offset = this.GetYOffset(item);
                if (Math.Abs(offset) <= Math.Abs(minOffset))
                {
                    minOffset = offset;
                    topItem = item;
                }
            }

            return topItem;
        }

        private void SetGroupItemOffset(ContentPresenter groupHeader, double offset)
        {
            if (groupHeader.RenderTransform as TranslateTransform == null)
                groupHeader.RenderTransform = new TranslateTransform();

            Panel.SetZIndex(groupHeader, 999);
            ((TranslateTransform)groupHeader.RenderTransform).Y -= offset;
        }

        private double GetYOffset(UIElement uiElement)
        {
            var transform = (MatrixTransform)uiElement.TransformToVisual(this.AssociatedObject);
            return transform.Matrix.OffsetY;
        }
    }
}
