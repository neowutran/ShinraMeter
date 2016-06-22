using System;
using System.Windows;
using System.Windows.Input;
using Data;
using Tera.Game;
using DamageMeter.Database.Structures;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailMana
    {
        public SkillDetailMana(Tera.Game.Skill skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            InitializeComponent();
            Update(skill, skills, playerDealt, entityInformation, timedEncounter);
        }

        public void Update(Tera.Game.Skill skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            //TODO Need to refactor this shitty copy paste shit
            bool? chained = skill?.IsChained;
            string hit = skill?.Detail;

            if (hit == null)
            {
                if (BasicTeraData.Instance.HotDotDatabase.Get(skill.Id) != null)
                {
                    hit = "MOT";
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

            LabelName.ToolTip = skill.Id;
            LabelNumberHitMana.Content = skills.Hits(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter);
            LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(skills.Amount(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
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