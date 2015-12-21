using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.Skills.Skill;
using DamageMeter.Skills.Skill.SkillDetail;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class Skill
    {
        public Skill(DamageMeter.Skills.Skill.Skill skill, SkillStats stats)
        {
            InitializeComponent();

            LabelName.Content = skill.SkillName;
            Update(skill, stats);
        }

        public string SkillNameIdent => (string) LabelName.Content;

        public void Update(DamageMeter.Skills.Skill.Skill skill, SkillStats stats)
        {
            var skillsId = "";
            for (var i = 0; i < skill.SkillId.Count; i++)
            {
                skillsId += skill.SkillId[i];
                if (i < skill.SkillId.Count - 1)
                {
                    skillsId += ",";
                }
            }

            LabelId.Content = skillsId;
            LabelCritRateDmg.Content = stats.CritRateDmg + "%";
            LabelCritRateHeal.Content = stats.CritRateHeal + "%";

            LabelDamagePercentage.Content = stats.DamagePercentage + "%";
            LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(stats.Damage);

            LabelNumberHitDmg.Content = stats.HitsDmg;
            LabelNumberHitHeal.Content = stats.HitsHeal;
            LabelNumberHitMana.Content = stats.HitsMana;


            LabelNumberCritDmg.Content = stats.CritsDmg;
            LabelNumberCritHeal.Content = stats.CritsHeal;

            LabelTotalHeal.Content = FormatHelpers.Instance.FormatValue(stats.Heal);
            LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(stats.Mana);
            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue(stats.AverageCrit);
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(stats.BiggestCrit);
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue(stats.AverageHit);
            LabelAverageTotal.Content = FormatHelpers.Instance.FormatValue(stats.AverageTotal);


            IEnumerable<KeyValuePair<int, SkillDetailStats>> listStats = stats.SkillDetails.ToList();
            listStats = listStats.OrderByDescending(stat => stat.Value.Damage);
            SkillsDetailList.Items.Clear();
            foreach (var stat in listStats)
            {
                SkillsDetailList.Items.Add(new SkillDetail(stat.Value));
            }
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