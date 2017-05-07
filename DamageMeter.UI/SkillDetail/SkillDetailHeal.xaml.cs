using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailHeal
    {
        public SkillDetailHeal(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            InitializeComponent();
            Update(skill, skillAggregate);
        }

        public void Update(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            var chained = skill.IsChained;
            var hit = skill.Detail;

            if (skill.IsHotDot)
                hit = LP.Hot;
            if (hit != null)
                LabelName.Content = hit;
            if (chained == true)
                LabelName.Content += " " + LP.Chained;

            LabelName.ToolTip = skill.Id;
            LabelCritRateHeal.Content = skillAggregate.CritRate(skill.Id) + "%";
            LabelNumberHitHeal.Content = skillAggregate.Hits(skill.Id);
            LabelNumberCritHeal.Content = skillAggregate.Crits(skill.Id);
            LabelTotalHeal.Content = FormatHelpers.Instance.FormatValue(skillAggregate.Amount(skill.Id));
            LabelBiggestHit.Content = FormatHelpers.Instance.FormatValue(skillAggregate.BiggestHit(skill.Id));
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(skillAggregate.BiggestCrit(skill.Id));
            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue((long) skillAggregate.AvgCrit(skill.Id));
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long) skillAggregate.AvgWhite(skill.Id));
            LabelAverage.Content = FormatHelpers.Instance.FormatValue((long) skillAggregate.Avg(skill.Id));
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}