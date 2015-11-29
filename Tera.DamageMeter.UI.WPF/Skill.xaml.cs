using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class Skill
    {
        public static readonly string SkillName = "Skill name";
        public static readonly string SkillId = "Skill Id";
        public static readonly string CritRate = "CRdmg + CRheal";
        public static readonly string TotalDamage = "Dmg";
        public static readonly string DamagePercentage = "% Dmg";
        public static readonly string AverageCrit = "Avg Crit";
        public static readonly string BiggestCrit = "Big Crit";
        public static readonly string LowestCrit = "Low Crit";
        public static readonly string AverageHit = "Avg Blk";
        public static readonly string Hits = "HDmg + HHeal + HMana";
        public static readonly string Crits = "CritsDmg + CritsHeal";
        public static readonly string Mana = "Mana";


        public Skill(DamageMeter.Skill skill, SkillStats stats, bool template = false)
        {
            InitializeComponent();

            if (template)
            {
                LabelName.Content = SkillName;
                LabelId.Content = SkillId;
                LabelCritRate.Content = CritRate;
                LabelTotalDamage.Content = TotalDamage + "↓";
                LabelDamagePercentage.Content = DamagePercentage;
                LabelNumberHit.Content = Hits;
                LabelAverageCrit.Content = AverageCrit;
                LabelBiggestCrit.Content = BiggestCrit;
                LabelLowestCrit.Content = LowestCrit;
                LabelAverageHit.Content = AverageHit;
                LabelNumberCrit.Content = Crits;
                LabelTotalMana.Content = Mana;

                LabelName.Foreground = Brushes.Red;
                LabelDamagePercentage.Foreground = Brushes.Red;
                LabelId.Foreground = Brushes.Red;
                LabelCritRate.Foreground = Brushes.Red;
                LabelTotalDamage.Foreground = Brushes.Red;
                LabelNumberHit.Foreground = Brushes.Red;
                LabelAverageCrit.Foreground = Brushes.Red;
                LabelBiggestCrit.Foreground = Brushes.Red;
                LabelLowestCrit.Foreground = Brushes.Red;
                LabelAverageHit.Foreground = Brushes.Red;
                LabelNumberCrit.Foreground = Brushes.Red;
                LabelTotalMana.Foreground = Brushes.Red;
            }
            else
            {
                LabelName.Content = skill.SkillName;

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
                LabelCritRate.Content = stats.CritRateDmg + "% + " + stats.CritRateHeal + "%";
                LabelDamagePercentage.Content = stats.DamagePercentage + "%";
                LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(stats.Damage);
                LabelNumberHit.Content = stats.HitsDmg + " + " + stats.HitsHeal + " + " + stats.HitsMana;
                LabelNumberCrit.Content = stats.CritsDmg + " + " + stats.CritsHeal;
                LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(stats.Mana);
                LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue(stats.AverageCrit);
                LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(stats.BiggestCrit);
                LabelLowestCrit.Content = FormatHelpers.Instance.FormatValue(stats.LowestCrit);
                LabelAverageHit.Content = FormatHelpers.Instance.FormatValue(stats.AverageHit);
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
    }
}