using System;
using System.Globalization;
using System.Windows.Data;

namespace Nanopath.View
{
    /// <summary>
    /// VersionStringToPresentationStringConverter
    /// Converts a software version string into a presentable string containing the version number
    /// </summary>
    public class VersionStringToPresentationStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string convertedValue = "Failed to convert value.";
            if (value is string)
            {
                convertedValue = $"Software Version {value}";
            }
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}