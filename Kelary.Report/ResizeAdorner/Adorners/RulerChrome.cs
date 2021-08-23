using System.Windows;
using System.Windows.Controls;

namespace Kelary.Report.ResizeAdorner
{
    public class RulerChrome : Control
    {
        static RulerChrome()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RulerChrome), new FrameworkPropertyMetadata(typeof(RulerChrome)));
        }
    }
}
