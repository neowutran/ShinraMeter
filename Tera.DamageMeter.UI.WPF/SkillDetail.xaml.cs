using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tera.DamageMeter.Skills.Skill.SkillDetail;
using Tera.Data;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetail : UserControl
    {
        public SkillDetail(SkillDetailStats skill)
        {
            InitializeComponent();
            Update(skill);
        }

        public void Update(SkillDetailStats skill)
        {
            var hit = BasicTeraData.Instance.SkillDatabase.Hit(skill.PlayerInfo.Class, skill.Id);
            var chained = BasicTeraData.Instance.SkillDatabase.IsChained(skill.PlayerInfo.Class, skill.Id);
            if (hit != null)
            {
                LabelName.Content = hit;
            }
            if (chained == true)
            {
                LabelName.Content += ": Chained";
            }

            LabelId.Content = skill.Id;
            LabelCritRateDmg.Content = skill.CritRateDmg + "%";
            LabelCritRateHeal.Content = skill.CritRateHeal + "%";

            LabelDamagePercentage.Content = skill.DamagePercentage + "%";
            LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(skill.Damage);

            LabelNumberHitDmg.Content = skill.HitsDmg;
            LabelNumberHitHeal.Content = skill.HitsHeal;
            LabelNumberHitMana.Content = skill.HitsMana;

            LabelNumberCritDmg.Content = skill.CritsDmg;
            LabelNumberCritHeal.Content = skill.CritsHeal;

            LabelTotalHeal.Content = FormatHelpers.Instance.FormatValue(skill.Heal);
            LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(skill.Mana);
            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue(skill.AverageCrit);
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(skill.BiggestCrit);
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue(skill.AverageHit);


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