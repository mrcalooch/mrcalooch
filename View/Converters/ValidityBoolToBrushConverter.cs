using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nanopath.View
{
    /// <summary>
    /// ValidityBoolToBrushConverter
    /// Converts an boolean enum to a brush 
    /// </summary>
    public class ValidityBoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush convertedValue = new SolidColorBrush(Colors.Gray);
            if (value is bool b)
            {
                if (b) convertedValue = new SolidColorBrush(Colors.Gray);
                else convertedValue = new SolidColorBrush(Colors.Red);
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
