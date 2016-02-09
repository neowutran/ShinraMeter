using System;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuff
    {
        private readonly EntityStatsMain _parent;

        public EnduranceDebuff(EntityStatsMain parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        public void Update(HotDot hotdot, AbnormalityDuration abnormalityDuration, EntityInfo entityInfo)
        {
            LabelClass.Content = abnormalityDuration.InitialPlayerClass;

            var second = abnormalityDuration.Duration/TimeSpan.TicksPerSecond;
            var interval = TimeSpan.FromSeconds(second);
            LabelAbnormalityDuration.Content = interval.ToString(@"mm\:ss");

            LabelAbnormalityDurationPercentage.Content = abnormalityDuration.Duration*100/entityInfo.Interval + "%";

            second = entityInfo.Interval/TimeSpan.TicksPerSecond;
            interval = TimeSpan.FromSeconds(second);
            LabelInterval.Content = interval.ToString(@"mm\:ss");

            LabelName.Content = hotdot.Name;
            LabelId.Content = hotdot.Id;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _parent.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}