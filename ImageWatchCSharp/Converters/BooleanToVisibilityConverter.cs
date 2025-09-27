using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageWatchCSharp.Converters
{

    public class BooleanToVisibilityConverter : IValueConverter
    {

        public bool UseCollapsed { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        public bool Inverse { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is bool nullable )
            {
                bValue = nullable;
            }
            else if (value != null)
            {
                bValue = System.Convert.ToBoolean(value);
            }
            if (Inverse)
                bValue = !bValue;

            return bValue ? Visibility.Visible : (UseCollapsed ? Visibility.Collapsed : Visibility.Hidden);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = (visibility == Visibility.Visible);
                return Inverse ? !result : result;
            }

            return false;
        }
    }
}