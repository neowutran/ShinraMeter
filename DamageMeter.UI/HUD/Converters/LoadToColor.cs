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
            if (!(value is int val)) return new SolidColorBrush(Color.FromArgb(0x22, 0xff, 0xff, 0xff));

            return val > 1000 && val < 5000
                ? Brushes.DarkOrange
                : val >= 5000
                    ? new SolidColorBrush(Color.FromRgb(0xff, 30, 0x40))
                    : new SolidColorBrush(Color.FromArgb(0x22, 0xff, 0xff, 0xff));

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LoadToAngle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int val)) return 0;
            var min = Math.Min(5000D, val);
            var ret =  (min/ 5000D) * 359.99;
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}