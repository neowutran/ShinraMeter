using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace DamageMeter.UI.HUD.Converters
{
    public class TransparencyToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value) { return new System.Windows.Thickness(2,2,5,5); }
            return new System.Windows.Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MathMultiplicationConverter : MarkupExtension, IValueConverter
    {
        public double Multiplier { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double?)value;
            return val * Multiplier;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

}