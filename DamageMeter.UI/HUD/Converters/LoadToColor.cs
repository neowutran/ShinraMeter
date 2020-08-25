using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DamageMeter.UI.HUD.Converters
{
    public class LoadToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int val)) return Brushes.Transparent;

            return val > 1000 && val < 5000
                ? Brushes.DarkOrange
                : val >= 5000
                    ? new SolidColorBrush(Color.FromRgb(0xff, 30, 0x40))
                    : Brushes.Transparent;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}