using System;
using System.Windows;
using System.Windows.Input;
using Data;
using Lang;
using Tera.Game;
using Tera.Game.Abnormality;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuffDetail
    {
        public EnduranceDebuffDetail(HotDot hotdot, int stack, AbnormalityDuration abnormalityDuration, long firstHit, long lastHit)
        {
            InitializeComponent();
            Update(hotdot, stack, abnormalityDuration, firstHit, lastHit);
        }

        public void Update(HotDot hotdot, int stack, AbnormalityDuration abnormalityDuration, long firstHit, long lastHit)
        {
            SkillIcon.ImageSource = BasicTeraData.Instance.Icons.GetImage(hotdot.IconName);
            SkillIconWrapper.ToolTip = string.IsNullOrEmpty(hotdot.ItemName) ? null : hotdot.ItemName;
            LabelClass.Content = LP.ResourceManager.GetString(abnormalityDuration.InitialPlayerClass.ToString(), LP.Culture);
            var intervalEntity = lastHit - firstHit;
            var ticks = abnormalityDuration.Duration(firstHit, lastHit, stack);
            var interval = TimeSpan.FromTicks(ticks);
            LabelAbnormalityDuration.Content = interval.ToString(@"mm\:ss");

            if (intervalEntity == 0) { LabelAbnormalityDurationPercentage.Content = "0%"; }
            else { LabelAbnormalityDurationPercentage.Content = abnormalityDuration.Duration(firstHit, lastHit, stack) * 100 / intervalEntity + "%"; }
            interval = TimeSpan.FromTicks(intervalEntity);
            LabelInterval.Content = interval.ToString(@"mm\:ss");

            LabelName.Content = stack;
            LabelName.ToolTip = string.IsNullOrEmpty(hotdot.Tooltip) ? null : hotdot.Tooltip;
            LabelAbnormalityDurationPercentage.ToolTip = hotdot.Id;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}