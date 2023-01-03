using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Nanopath.View
{
    /// <summary>
    /// InverseBoolToVisibilityConverter
    /// Like the traditional BoolToVisibilityConverter, only inverse.
    /// Will set visibility to HIDDEN if bool is true
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility convertedValue = Visibility.Visible;
            if (value is bool b)
            {
                if (b) convertedValue = Visibility.Hidden;
            }
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
