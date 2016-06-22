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
    public partial class SkillDetailHeal
    {
        public SkillDetailHeal(Tera.Game.Skill skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            InitializeComponent();
            Update(skill, skills, playerDealt, entityInformation, timedEncounter);
        }

        public void Update(Tera.Game.Skill skill, Database.Structures.Skills skills, PlayerDealt playerDealt, EntityInformation entityInformation, bool timedEncounter)
        {
            bool? chained = skill?.IsChained;
            string hit = skill?.Detail;

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


            LabelName.ToolTip = skill.Id;
            LabelCritRateHeal.Content = skills.CritRate(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter) + "%";
            LabelNumberHitHeal.Content = skills.Hits(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter);
            LabelNumberCritHeal.Content = skills.Crits(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter);
            LabelTotalHeal.Content = FormatHelpers.Instance.FormatValue(skills.Amount(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
            LabelBiggestHit.Content = FormatHelpers.Instance.FormatValue((long)skills.BiggestHit(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
            LabelBiggestCrit.Content = FormatHelpers.Instance.FormatValue((long)skills.BiggestCrit(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
            LabelAverageCrit.Content = FormatHelpers.Instance.FormatValue((long)skills.AverageCrit(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
            LabelAverageHit.Content = FormatHelpers.Instance.FormatValue((long)skills.AverageWhite(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
            LabelAverage.Content = FormatHelpers.Instance.FormatValue((long)skills.Average(playerDealt.Source.User.Id, entityInformation.Entity.Id, skill.Id, timedEncounter));
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