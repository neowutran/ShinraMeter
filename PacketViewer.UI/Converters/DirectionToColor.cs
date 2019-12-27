using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Tera;

namespace PacketViewer.UI
{
    public class DirectionToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (MessageDirection)value;
            var c = Colors.Gray;
            if (v == MessageDirection.ServerToClient) c = Color.FromArgb(0xcc, Colors.DodgerBlue.R, Colors.DodgerBlue.G, Colors.DodgerBlue.B);
            if (v == MessageDirection.ClientToServer) c = Color.FromArgb(0xcc, Colors.DarkOrange.R, Colors.DarkOrange.G, Colors.DarkOrange.B);
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}