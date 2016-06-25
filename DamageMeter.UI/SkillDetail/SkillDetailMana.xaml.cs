using System;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailMana
    {
        public SkillDetailMana(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            InitializeComponent();
            Update(skill, skillAggregate);
        }

        public void Update(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            var chained = skill.IsChained;
            var hit = skill.Detail;

            if (skill.IsHotDot)
            {
                hit = "DOT";
            }
            if (hit != null)
            {
                LabelName.Content = hit;
            }
            if (chained == true)
            {
                LabelName.Content += " Chained";
            }

            LabelName.ToolTip = skill.Id;
            LabelNumberHitMana.Content = skillAggregate.Hits(skill.Id);
            LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(skillAggregate.Amount(skill.Id));
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