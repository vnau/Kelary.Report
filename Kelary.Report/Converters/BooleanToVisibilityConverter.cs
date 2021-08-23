using System;

#if NETFX_CORE
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace Kelary.Report.Converters
{
    /// <summary>
    /// The boolean value to Visibility converter.
    /// Use parameter value True to inverse visibility.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {

#if NETFX_CORE

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return InternalConvert(value, targetType, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return InternalConvertBack(value, targetType, parameter);
        }

#else
        /// <summary>
        /// Perforn firward conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return InternalConvert(value, targetType, parameter);
        }

        /// <summary>
        /// Perform back conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return InternalConvertBack(value, targetType, parameter);
        }

#endif

        /// <summary>
        /// Perform internal forward conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private object InternalConvert(object value, Type targetType, object parameter)
        {
            try
            {
                var flag = false;
                if (value is bool)
                {
                    flag = (bool)value;
                }
                else if (value is bool?)
                {
                    var nullable = (bool?)value;
                    flag = nullable.GetValueOrDefault();
                }
                if (parameter != null)
                {
                    if (bool.Parse((string)parameter))
                    {
                        flag = !flag;
                    }
                }
                if (flag)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Perform internal back conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public object InternalConvertBack(object value, Type targetType, object parameter)
        {
            var back = ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
            if (parameter != null)
            {
                if ((bool)parameter)
                {
                    back = !back;
                }
            }
            return back;
        }
    }

}
