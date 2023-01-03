using System;
using System.Globalization;
using System.Windows.Data;

namespace Nanopath.View
{
    /// <summary>
    /// PosNegToStringConverter
    /// Converts an PosNeg enum to a string 
    /// </summary>
    public class PosNegToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string convertedValue = "?";
            if (value is PosNeg s)
            {
                if (s == PosNeg.Negative) convertedValue = "NEG";
                else if (s == PosNeg.Positive) convertedValue = "POS";
            }
            
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
