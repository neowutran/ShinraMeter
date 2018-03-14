using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailDps
    {
        public SkillDetailDps(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            InitializeComponent();
            Update(skill, skillAggregate);
        }

        public void Update(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            var chained = skill.IsChained;
            var hit = skill.Detail;

            if (skill.IsHotDot) { hit = LP.Dot; }

            if (hit != null) { LabelName.Content = hit; }
            if (chained == true) { LabelName.Content += " " + LP.Chained; }

            LabelName.ToolTip = skill.Id;
            LabelCritRateDmg.Content = skillAggregate.CritRate(skill.Id) + "%";

            LabelDamagePercentage.Content = skillAggregate.DamagePercent(skill.Id) + "%";
            LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(skillAggregate.Amount(skill.Id));

            var hits = skillAggregate.Hits(skill.Id);
            LabelNumberHitDmg.Content = hits;
            LabelNumberCritDmg.Content = skillAggregate.Crits(skill.Id);

            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue((long) skillAggregate.AvgCrit(skill.Id));
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(skillAggregate.BiggestCrit(skill.Id));
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long) skillAggregate.AvgWhite(skill.Id));
            LabelAverageTotal.Content = FormatHelpers.Instance.FormatValue((long) skillAggregate.Avg(skill.Id));
            LabelNumberHPM.Content = FormatHelpers.Instance.FormatDouble(skillAggregate.Interval == 0 ? 0 : (double)hits / skillAggregate.Interval / TimeSpan.TicksPerMinute);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}