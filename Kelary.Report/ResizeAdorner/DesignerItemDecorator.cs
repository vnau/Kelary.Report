using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace Kelary.Report.ResizeAdorner
{
    public class DesignerItemDecorator : Control
    {
        private Adorner adorner;

        public bool ShowDecorator
        {
            get
            {
                return (bool)GetValue(ShowDecoratorProperty);
            }
            set
            {
                SetValue(ShowDecoratorProperty, value);
            }
        }

        public static readonly DependencyProperty ShowDecoratorProperty =
             DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(DesignerItemDecorator),
             new FrameworkPropertyMetadata(false, new PropertyChangedCallback(ShowDecoratorProperty_Changed)));

        public DesignerItemDecorator()
        {
            Loaded += DesignerItemDecorator_Loaded;
            Unloaded += new RoutedEventHandler(this.DesignerItemDecorator_Unloaded);
        }

        private void DesignerItemDecorator_Loaded(object sender, RoutedEventArgs e)
        {
            // Show adorner if focus has been enabled before.
            ContentControl designerItem = this.DataContext as ContentControl;
            if (designerItem == null)
                return;
            if (designerItem.IsFocused)
                ShowAdorner();
        }

        private void HideAdorner()
        {
            if (this.adorner != null)
            {
                this.adorner.Visibility = Visibility.Hidden;
            }
        }

        private void ShowAdorner()
        {
            if (this.adorner == null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (adornerLayer != null)
                {
                    if (this.DataContext is ContentControl designerItem)
                    {
                        Canvas canvas = VisualTreeHelper.GetParent(designerItem) as Canvas;
                        this.adorner = new ResizeAdorner(designerItem);
                        adornerLayer.Add(this.adorner);

                        if (this.ShowDecorator)
                        {
                            this.adorner.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.adorner.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
            else
            {
                this.adorner.Visibility = Visibility.Visible;
            }
        }

        private void DesignerItemDecorator_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.adorner != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(this.adorner);
                }

                this.adorner = null;
            }
        }

        private static void ShowDecoratorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DesignerItemDecorator decorator = (DesignerItemDecorator)d;
            bool showDecorator = (bool)e.NewValue;

            if (showDecorator)
            {
                decorator.ShowAdorner();
            }
            else
            {
                decorator.HideAdorner();
            }
        }
    }
}
