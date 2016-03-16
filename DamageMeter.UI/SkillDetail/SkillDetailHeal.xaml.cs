using System;
using System.Windows;
using System.Windows.Input;
using DamageMeter.Skills.Skill.SkillDetail;
using Data;
using Tera.Game;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailHeal
    {
        public SkillDetailHeal(SkillDetailStats skill)
        {
            InitializeComponent();
            Update(skill);
        }

        public void Update(SkillDetailStats skill)
        {
            var userskill = BasicTeraData.Instance.SkillDatabase.Get(new UserEntity(skill.PlayerInfo.Player.User.Id), skill.Id);
            bool? chained = false;
            string hit = null;
            if (userskill != null)
            {
                hit = ((UserSkill)userskill).Hit;
                chained = userskill.IsChained;
            }
            if (hit == null)
            {
                if (BasicTeraData.Instance.HotDotDatabase.Get(skill.Id) != null)
                {
                    hit = "HOT";
                }
            }
            if (hit != null)
            {
                LabelName.Content = hit;
            }
            if (chained == true)
            {
                LabelName.Content += " Chained";
            }


            LabelId.Content = skill.Id;
            LabelCritRateHeal.Content = skill.CritRateHeal + "%";


            LabelNumberHitHeal.Content = skill.HitsHeal;
            LabelNumberCritHeal.Content = skill.CritsHeal;

            LabelTotalHeal.Content = FormatHelpers.Instance.FormatValue(skill.Heal);
            LabelBiggestHit.Content = FormatHelpers.Instance.FormatValue(skill.HealBiggestHit);
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue(skill.HealBiggestCrit);


            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue(skill.HealAverageCrit);
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue(skill.HealAverageHit);
            LabelAverage.Content = FormatHelpers.Instance.FormatValue(skill.HealAverageTotal);
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