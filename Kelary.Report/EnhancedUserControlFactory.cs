using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Kelary.Report
{
    /// <summary>
    /// Document controls class factory
    /// </summary>
    public class EnhancedUserControlFactory
    {
        private List<Type> Types = new List<Type>();

        /// <summary>
        /// Convert GUID to control's name.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static string NameFromGuid(Guid guid)
        {
            string name = string.Format("{0}{1}", "Control_", guid.ToString().Replace('-', '_'));
            return name;
        }

        public void AddType(Type controlType)
        {
            Types.Add(controlType);
        }

        /// <summary>
        /// Create control instance
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public UIElement CreateControl(Type controlType, params object[] args)
        {
            foreach (var type in Types)
            {
                if (controlType == type)
                {
                    //Type controlType = GetType(name);
                    IEnhancedUserControl viewmodel = null;
                    viewmodel = Activator.CreateInstance(controlType, args) as IEnhancedUserControl;

                    string name = NameFromGuid(Guid.NewGuid());
                    var control = new ContainerControl()
                    {
                        DataContext = viewmodel,
                        Name = name
                    };

                    viewmodel.Name = name;

                    //control.Name = NameFromGuid(Guid.NewGuid());
                    UIElement newControl = control;// as Control;

                    newControl.GotFocus += Control_GotFocus;
                    newControl.LostFocus += Control_LostFocus;
                    // Change the name of the new control so that it won't
                    // overwrite another control's data.
                    //newControl.Name = name;

                    ControlCreated?.Invoke(viewmodel, new RoutedEventArgs());
                    return newControl;
                }
            }

            //Debug.Assert(control != null);

            return null;
        }

        public event RoutedEventHandler ControlCreated;

        /// <summary>
        /// Occurs when one of created controls gets logical focus.
        /// </summary>
        public event RoutedEventHandler GotFocus;

        /// <summary>
        /// Occurs when one of created controls loses logical focus.
        /// </summary>
        public event RoutedEventHandler LostFocus;

        private void Control_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            // If Control has a DataSource (ViewModel) then use it as sender.
            if (element.DataContext is IEnhancedUserControl)
                GotFocus?.Invoke(element.DataContext, e);
            if (element is IEnhancedUserControl)
                GotFocus?.Invoke(element, e);
        }

        private void Control_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            // If Control has a DataSource (ViewModel) then use it as sender.
            if (element.DataContext is IEnhancedUserControl)
                LostFocus?.Invoke(element.DataContext, e);
            if (element is IEnhancedUserControl)
                LostFocus?.Invoke(element, e);
        }

        public string[] ControlTypeNames
        {
            get
            {
                //Type[] controlTypes = { typeof(PhotoControl), typeof(TimeStampControl) };
                string[] controlTypeNames = Types.Select(type =>
                EnhancedUserControlUtils.ShortNamespace(type.Namespace) + ":" + type.Name).ToArray();
                return controlTypeNames;
            }
        }

    }
}
