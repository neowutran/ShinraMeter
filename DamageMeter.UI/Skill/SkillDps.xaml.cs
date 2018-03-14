using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.UI.SkillDetail;
using Data;

namespace DamageMeter.UI.Skill
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillDps : ISkill
    {
        public SkillDps(SkillAggregate skill)
        {
            InitializeComponent();
            LabelName.Content = skill.Name;

            foreach (var skillInfo in skill.Skills)
            {
                if (string.IsNullOrEmpty(skillInfo.Key.IconName)) { continue; }
                SkillIcon.ImageSource = BasicTeraData.Instance.Icons.GetImage(skillInfo.Key.IconName);
                break;
            }
            Update(skill);
        }

        public void Update(SkillAggregate skill)
        {
            var skillsId = skill.Id();

            LabelName.ToolTip = skillsId;
            LabelCritRateDmg.Content = skill.CritRate() + "%";

            LabelDamagePercentage.Content = skill.DamagePercent() + "%";
            LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(skill.Amount());

            var hits = skill.Hits();
            LabelNumberHitDmg.Content = hits;

            LabelNumberCritDmg.Content = skill.Crits();

            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue((long) skill.AvgCrit());
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(skill.BiggestCrit());
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long) skill.AvgWhite());
            LabelAverageTotal.Content = FormatHelpers.Instance.FormatValue((long) skill.Avg());
            LabelNumberHPM.Content = FormatHelpers.Instance.FormatDouble(skill.Interval == 0 ? 0 : (double)hits / skill.Interval / TimeSpan.TicksPerMinute);

            SkillsDetailList.Items.Clear();
            foreach (var skillInfo in skill.Skills) { SkillsDetailList.Items.Add(new SkillDetailDps(skillInfo.Key, skill)); }
        }

        public string SkillNameIdent()
        {
            return (string) LabelName.Content;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Background = Brushes.Transparent;
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(Color.FromArgb(0x10, 255, 255, 255));
        }
    }
}