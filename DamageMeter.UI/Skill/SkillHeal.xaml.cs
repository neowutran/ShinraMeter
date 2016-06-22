using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DamageMeter.UI.SkillDetail;
using Data;
using DamageMeter.Database.Structures;

namespace DamageMeter.UI.Skill
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillHeal : ISkill
    {
        public SkillHeal(SkillAggregate skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            InitializeComponent();
            LabelName.Content = skill.Name;

            foreach (var skillInfo in skill.Skills)
            {
                if (!string.IsNullOrEmpty(skillInfo.IconName))
                {
                    SkillIcon.Source = BasicTeraData.Instance.Icons.GetImage(skillInfo.IconName);
                    break;
                }
            }
            Update(skill, skills, playerDealt, entityInformation, timedEncounter);
        }

        public void Update(SkillAggregate skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            var skillsId = skill.Id();

            LabelName.ToolTip = skillsId;
            LabelCritRateHeal.Content = skill.CritRate() + "%";


            LabelNumberHitHeal.Content = skill.Hits();
            LabelNumberCritHeal.Content = skill.Crits();

            LabelTotalHeal.Content = FormatHelpers.Instance.FormatValue(skill.Amount());
            LabelBiggestHit.Content = FormatHelpers.Instance.FormatValue( (long)skill.BiggestHit());
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue( (long)skill.BiggestCrit());


            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue((long)skill.AvgCrit());
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long)skill.AvgHit());
            LabelAverage.Content = FormatHelpers.Instance.FormatValue((long)skill.Avg());


            SkillsDetailList.Items.Clear();

            foreach (var skillInfo in skill.Skills)
            {
                SkillsDetailList.Items.Add(new SkillDetailHeal(skillInfo, skills, playerDealt, entityInformation, timedEncounter));
            }

        }

        public string SkillNameIdent()
        {
            return (string) LabelName.Content;
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