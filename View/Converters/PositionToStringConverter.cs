using System;
using System.Globalization;
using System.Windows.Data;

namespace Nanopath.View
{
    /// <summary>
    /// PositionToStringConverter
    /// Converts ant double? to a string ("X" for null), and also converts back from a string to
    /// an int? (if the string does not contain a number the int? will be null)
    /// </summary>
    public class PositionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string convertedValue = "?";
            double? toConvert = (double?) value;                                // See if the value is an double?
            convertedValue = toConvert.HasValue ? toConvert.Value.ToString("F2") : "X";   // If it's not null parse the value, otherwise make it "X"
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? convertedValue = null;                                                 // Default to null
            if (value is string s)
            {
                convertedValue = Utilities.StringToLocation(s, out var coerced);
            }
            return convertedValue;
        }
    }
}
