using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Data;

namespace DamageMeter.UI
{
    public class StatTemplateSelector : MarkupExtension, IValueConverter
    {
        public DataTemplate DamagePerc { get; set; }
        public DataTemplate Damage { get; set; }
        public DataTemplate Dps { get; set; }
        public DataTemplate CritRate { get; set; }
        public DataTemplate HealCritRate { get; set; }
        public DataTemplate DamageFromCrits { get; set; }
        public DataTemplate DamageTaken { get; set; }
        public DataTemplate DpsTaken { get; set; }
        public DataTemplate Deaths { get; set; }
        public DataTemplate Floortime { get; set; }
        public DataTemplate FloortimePerc { get; set; }
        public DataTemplate AggroPerc { get; set; }
        public DataTemplate EnrageCasts { get; set; }
        public DataTemplate Hps { get; set; }
        public DataTemplate EnduDebuffUptime { get; set; }
        public DataTemplate HealerUptimes { get; set; }
        public DataTemplate Empty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Metric vm)) return null;

            return vm switch
            {
                Metric.DamagePerc => DamagePerc,
                Metric.Damage => Damage,
                Metric.Dps => Dps,
                Metric.CritRate => CritRate,
                Metric.DamageFromCrits => DamageFromCrits,
                Metric.DamageTaken => DamageTaken,
                Metric.DpsTaken => DpsTaken,
                Metric.Deaths => Deaths,
                Metric.Floortime => Floortime,
                Metric.FloortimePerc => FloortimePerc,
                Metric.AggroPerc => AggroPerc,
                Metric.EnrageCasts => EnrageCasts,
                Metric.HealCritRate => HealCritRate,
                Metric.Hps => Hps,
                Metric.EnduDebuffUptime => EnduDebuffUptime,
                Metric.HealerUptimes => HealerUptimes,
                _ => Empty
            };
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