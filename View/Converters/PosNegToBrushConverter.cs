using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nanopath.View
{
    /// <summary>
    /// PosNegToStringConverter
    /// Converts an PosNeg enum to a brush 
    /// </summary>
    public class PosNegToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush convertedValue = new SolidColorBrush(Colors.Black);
            if (value is PosNeg s)
            {
                if (s == PosNeg.Negative) convertedValue = new SolidColorBrush(Colors.Red);
                else if (s == PosNeg.Positive) convertedValue = new SolidColorBrush(Colors.Green);
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
