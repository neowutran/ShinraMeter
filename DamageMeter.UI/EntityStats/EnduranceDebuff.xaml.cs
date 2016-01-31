using System;
using System.Windows.Controls;
using System.Windows.Input;
using Data;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuff : UserControl
    {
        public EnduranceDebuff(EntityStatsMain  parent, HotDot hotdot, AbnormalityDuration abnormalityDuration, EntityInfo entityInfo)
        {
            InitializeComponent();
            _parent = parent;

            LabelClass.Content = abnormalityDuration.InitialPlayerClass;
            LabelAbnormalityDuration.Content = Math.Round((double)(abnormalityDuration.Duration/ 10000000)) + "s";
            LabelAbnormalityDurationPercentage.Content = (abnormalityDuration.Duration * 100)/entityInfo.Interval + "%";
            LabelInterval.Content = Math.Round((double)(entityInfo.Interval/10000000))+"s";
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