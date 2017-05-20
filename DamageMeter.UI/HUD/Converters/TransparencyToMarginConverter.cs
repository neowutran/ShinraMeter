using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DamageMeter.UI.HUD.Converters
{
    public class TransparencyToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value) { return new System.Windows.Thickness(2,2,5,5); }
            return new System.Windows.Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}