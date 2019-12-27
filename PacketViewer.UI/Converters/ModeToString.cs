using System;
using System.Globalization;
using System.Windows.Data;

namespace PacketViewer.UI
{
    public class ModeToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var m = (FilterMode)value;
            switch (m)
            {
                case FilterMode.Exclude: return "HIDE";
                case FilterMode.ShowOnly: return "SHOW";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}