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
        public EnduranceDebuff(EntityStatsMain  parent)
        {
            InitializeComponent();
            _parent = parent;
        }

        public void Update(HotDot hotdot, AbnormalityDuration abnormalityDuration, EntityInfo entityInfo)
        {
            LabelClass.Content = abnormalityDuration.InitialPlayerClass;

            var interval = TimeSpan.FromSeconds(Math.Round((double)(abnormalityDuration.Duration / 10000000)));
            LabelAbnormalityDuration.Content = interval.ToString(@"mm\:ss");

            LabelAbnormalityDurationPercentage.Content = (abnormalityDuration.Duration * 100) / entityInfo.Interval + "%";

            interval = TimeSpan.FromSeconds(Math.Round((double)(entityInfo.Interval / 10000000)));
            LabelInterval.Content = interval.ToString(@"mm\:ss");

            LabelName.Content = hotdot.Name;
            LabelId.Content = hotdot.Id;
        }

        private readonly EntityStatsMain _parent;

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