using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Data;
using Lang;
using Tera.Game;
using Tera.Game.Abnormality;

namespace DamageMeter.UI.EntityStats
{
    /// <summary>
    ///     Logique d'interaction pour EnduranceDebuff.xaml
    /// </summary>
    public partial class EnduranceDebuff
    {
        public EnduranceDebuff()
        {
            InitializeComponent();
        }

        public void Update(HotDot hotdot, AbnormalityDuration abnormalityDuration, long firstHit, long lastHit)
        {
            SkillIcon.Source = BasicTeraData.Instance.Icons.GetImage(hotdot.IconName);
            SkillIcon.ToolTip = string.IsNullOrEmpty(hotdot.ItemName) ? null : hotdot.ItemName;
            LabelClass.Content = LP.ResourceManager.GetString(abnormalityDuration.InitialPlayerClass.ToString(), LP.Culture);
            var intervalEntity = lastHit - firstHit;
            var ticks = abnormalityDuration.Duration(firstHit, lastHit);
            var interval = TimeSpan.FromTicks(ticks);
            LabelAbnormalityDuration.Content = interval.ToString(@"mm\:ss");

            if (intervalEntity == 0) { LabelAbnormalityDurationPercentage.Content = "0%"; }
            else { LabelAbnormalityDurationPercentage.Content = abnormalityDuration.Duration(firstHit, lastHit) * 100 / intervalEntity + "%"; }
            interval = TimeSpan.FromTicks(intervalEntity);
            LabelInterval.Content = interval.ToString(@"mm\:ss");

            LabelName.Content = hotdot.Name;
            LabelName.ToolTip = string.IsNullOrEmpty(hotdot.Tooltip) ? null : hotdot.Tooltip;
            LabelAbnormalityDurationPercentage.ToolTip = hotdot.Id;
            StacksDetailList.Items.Clear();
            foreach (var stack in abnormalityDuration.Stacks(firstHit, lastHit))
            {
                StacksDetailList.Items.Add(new EnduranceDebuffDetail(hotdot, stack, abnormalityDuration, firstHit, lastHit));
            }
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var w = Window.GetWindow(this);
                w?.DragMove();
            }
            catch { Console.WriteLine(@"Exception move"); }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Background = Brushes.Transparent;
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Background = Brushes.Black;
        }
    }
}