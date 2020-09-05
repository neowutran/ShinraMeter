using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Data;

namespace DamageMeter.UI.HUD.Converters
{
    public class RoleToColorConverter : MarkupExtension, IValueConverter
    {
        public float Opacity { get; set; } = 1;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retCol = Colors.SlateGray;
            if (value is PlayerDamageViewModel.PlayerRole role)
            {
                retCol = role switch
                {
                    PlayerDamageViewModel.PlayerRole.Dps => BasicTeraData.Instance.WindowData.DpsColor,
                    PlayerDamageViewModel.PlayerRole.Tank => BasicTeraData.Instance.WindowData.TankColor,
                    PlayerDamageViewModel.PlayerRole.Healer => BasicTeraData.Instance.WindowData.HealerColor,
                    PlayerDamageViewModel.PlayerRole.Self => BasicTeraData.Instance.WindowData.PlayerColor,
                    _ => retCol
                };
            }

            retCol = Color.FromScRgb(Opacity, retCol.ScR, retCol.ScG, retCol.ScB);

            if (targetType == typeof(Brush)) return new SolidColorBrush(retCol);
            else return retCol;
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