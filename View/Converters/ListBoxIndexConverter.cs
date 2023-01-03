using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Nanopath.View
{
    /// <summary>
    /// ListBoxIndexConverter
    /// This converter takes in two bindings: a ListBox and a ListBox item
    /// XAML Example:
    ///     <MultiBinding Converter="{StaticResource ListBoxIndexConverter}">
    ///         <Binding RelativeSource = "{RelativeSource Mode=FindAncestor,AncestorType=ListBox}" />
    ///         <Binding RelativeSource="{RelativeSource Mode=FindAncestor,AncestorType=ListBoxItem}"/>
    ///     </MultiBinding>
    /// It returns the 1-based index of item in the ListBox's items
    /// This is useful for displaying the number of each item dynamically on the screen
    /// </summary>
    public class ListBoxIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is ListBox && values[1] is ListBoxItem)
            {
                ListBox lb = (ListBox) values[0];
                return lb.ItemContainerGenerator.IndexFromContainer((ListBoxItem) values[1]) +1;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
