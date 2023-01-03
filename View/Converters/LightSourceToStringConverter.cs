using System;
using System.Globalization;
using System.Windows.Data;

namespace Nanopath.View
{
    /// <summary>
    /// LightSourceToStringConverter
    /// Converts a double to a string, and also converts back from a string to a double
    /// </summary>
    public class LightSourceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string convertedValue = "0.00";
            if (value is double dVal)
            {
                convertedValue = dVal.ToString("F2");
            }
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double convertedValue = 0.0;
            if (value is string s)
            {
                double? dVal = Utilities.StringToLocation(s, out var coerced);
                if (dVal.HasValue) convertedValue = dVal.Value;
            }
            return convertedValue;
        }
    }
}
