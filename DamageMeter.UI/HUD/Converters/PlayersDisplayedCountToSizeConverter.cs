using System;
using System.Globalization;
using System.Windows.Data;

namespace DamageMeter.UI.HUD.Converters
{
    public class PlayersDisplayedCountToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int val)) val = 1;
            return val * 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}