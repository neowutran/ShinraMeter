using System;
using System.Windows;
using System.Windows.Input;
using DamageMeter.Skills.Skill.SkillDetail;
using Data;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailMana
    {
        public SkillDetailMana(SkillDetailStats skill)
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
                LabelName.Content += " Chained";
            }

            LabelId.Content = skill.Id;
            LabelNumberHitMana.Content = skill.HitsMana;
            LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(skill.Mana);
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