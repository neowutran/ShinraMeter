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
        public SkillDps(SkillAggregate skill)
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
            Update(skill);
        }

        private List<int> _ids = new List<int>();

        public void Update(SkillAggregate skill)
        {
          
            var skillsId = skill.Id();
           
            LabelName.ToolTip = skillsId;
            LabelCritRateDmg.Content = (skill.CritRate() * 100) + "%";

            LabelDamagePercentage.Content = (skill.DamagePercent() * 100) + "%";
            LabelTotalDamage.Content = FormatHelpers.Instance.FormatValue(skill.Amount());

            LabelNumberHitDmg.Content = skill.Hits();

            LabelNumberCritDmg.Content = skill.Crits();

            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue( (long)skill.AvgCrit());
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue( (long)skill.BiggestCrit());
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long)skill.AvgWhite());
            LabelAverageTotal.Content = FormatHelpers.Instance.FormatValue((long)skill.Avg());

            SkillsDetailList.Items.Clear();
             foreach (var skillInfo in skill.Skills)
            {
                SkillsDetailList.Items.Add(new SkillDetailDps( skillInfo , skill));
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