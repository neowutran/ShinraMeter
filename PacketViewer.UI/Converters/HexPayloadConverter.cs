using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PacketViewer.UI
{
    public class HexPayloadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var arr = (ArraySegment<byte>)value;
            var a = arr.ToArray();
            return BitConverter.ToString(a).Replace("-", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}