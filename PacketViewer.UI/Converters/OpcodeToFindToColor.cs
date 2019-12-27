using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PacketViewer.UI
{
    public enum OpcodeStatus
    {
        None,
        Mismatching,
        Confirmed
    }
    public class OpcodeToFindToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var opc = (OpcodeStatus)value;
            switch (opc)
            {
                case OpcodeStatus.Mismatching: return Brushes.Crimson;
                case OpcodeStatus.Confirmed: return Brushes.MediumSeaGreen;
            }
            return Brushes.SlateGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}