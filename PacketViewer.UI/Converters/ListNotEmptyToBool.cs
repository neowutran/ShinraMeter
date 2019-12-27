using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PacketViewer.UI
{
    public class ListNotEmptyToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (int)value;
            return (v > 0) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}