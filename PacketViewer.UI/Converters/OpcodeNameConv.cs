using System;
using System.Globalization;
using System.Windows.Data;

namespace PacketViewer.UI
{
    public class OpcodeNameConv : IValueConverter
    {
        private static OpcodeNameConv _instance;
        public static OpcodeNameConv Instance => _instance ?? (_instance = new OpcodeNameConv());
        public ObservableDictionary<ushort, OpcodeEnum> Known { get; set; } = new ObservableDictionary<ushort, OpcodeEnum>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //it's hex string
                var opn = (string) value;
                if (opn.Length == 4 && opn[1] != '_')
                {
                    if (Instance.Known.TryGetValue(System.Convert.ToUInt16(opn, 16), out OpcodeEnum opc)) { return opc.ToString(); }
                }
            }
            catch
            {
                //it's ushort
                var opn = (ushort) value; 
                if (Instance.Known.TryGetValue(opn, out OpcodeEnum opc)) { return opc.ToString(); }
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            Known.Clear();
        }
    }
}