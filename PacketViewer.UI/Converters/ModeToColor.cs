using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PacketViewer.UI
{
    public class ModeToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var m = (FilterMode) value;
            switch (m)
            {
                case FilterMode.Exclude: return new SolidColorBrush(Colors.Crimson);
                case FilterMode.ShowOnly: return  new SolidColorBrush(Colors.MediumSeaGreen);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}