using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PacketViewer.UI
{
    public class TextPayloadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var arr = (ArraySegment<byte>)value;
            var a = arr.ToArray();
            var sb = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                var c = (char)a[i];
                if (c > 0x1f && c < 0x80) sb.Append(c);
                else sb.Append("⋅");
                if ((i + 1) % 16 == 0) sb.Append("\n");
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}