using System;
using System.Windows;
using System.Windows.Input;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class Skill
    {
        public Skill(DamageMeter.Skill skill, SkillStats stats)
        {
            InitializeComponent();

            LabelName.Content = skill.SkillName;
            Update(skill, stats);
        }

        public string SkillNameIdent => (string) LabelName.Content;

        public void Update(DamageMeter.Skill skill, SkillStats stats)
        {
            var skillsId = "";
            for (var i = 0; i < skill.SkillId.Count; i++)
            {
                skillsId += +skill.SkillId[i];
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