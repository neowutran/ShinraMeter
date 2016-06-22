using System;
using System.Collections.Generic;
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
    public partial class SkillDps : ISkill
    {
        public SkillDps(SkillAggregate skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            InitializeComponent();
            LabelName.Content = skill.Name;

            foreach(var skillInfo in skill.Skills)
            {
                if (!string.IsNullOrEmpty(skillInfo.IconName))
                {
                    SkillIcon.Source = BasicTeraData.Instance.Icons.GetImage(skillInfo.IconName);
                    break;
                }
            }
            Update(skill, skills, playerDealt, entityInformation, timedEncounter);
        }

        private List<int> _ids = new List<int>();

        public void Update(SkillAggregate skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
          
            var skillsId = skill.Id();
           
            LabelName.ToolTip = skillsId;
            LabelCritRateDmg.Content = skill.CritRate() + "%";

            LabelDamagePercentage.Content = skill.DamagePercent() + "%";
            LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(skill.Amount());

            LabelNumberHitDmg.Content = skill.Hits();

            LabelNumberCritDmg.Content = skill.Crits();

            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue( (long)skill.AvgCrit());
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue( (long)skill.BiggestCrit());
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long)skill.AvgHit());
            LabelAverageTotal.Content = FormatHelpers.Instance.FormatValue((long)skill.Avg());

            SkillsDetailList.Items.Clear();
            foreach (var skillInfo in skill.Skills)
            {
                SkillsDetailList.Items.Add(new SkillDetailDps( skillInfo , skills, playerDealt, entityInformation, timedEncounter));
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