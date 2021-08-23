using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Kelary.Report.ResizeAdorner
{
    public class ResizeAdorner : Adorner
    {
        private VisualCollection visuals;
        private ResizeChrome chrome;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.visuals.Count;
            }
        }

        public ResizeAdorner(ContentControl designerItem)
                : base(designerItem)
        {
            SnapsToDevicePixels = true;
            chrome = new ResizeChrome()
            {
                DataContext = designerItem
            };
            visuals = new VisualCollection(this)
            {
                chrome
            };
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.visuals[index];
        }
    }
}
