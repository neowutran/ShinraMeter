using System;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillsHeaderHeal
    {
        public static readonly string SkillName = "Skill name";
        public static readonly string CritRateHeal = "% Crit";
        public static readonly string TotalDamage = "Dmg";
        public static readonly string DamagePercentage = "% Dmg";
        public static readonly string AverageCrit = "Avg Crit";
        public static readonly string BiggestCrit = "Max Crit";
        public static readonly string BiggestHit = "Max white";
        public static readonly string AverageHit = "Avg white";
        public static readonly string AverageTotal = "Avg";
        public static readonly string HitsHeal = "Hits";
        public static readonly string CritsHeal = "Crits";
        public static readonly string Heal = "Heal";

        public SkillsHeaderHeal()
        {
            InitializeComponent();
            LabelName.Content = SkillName;
            LabelCritRateHeal.Content = CritRateHeal;


            LabelNumberHitHeal.Content = HitsHeal;

            LabelNumberCritHeal.Content = CritsHeal;

            LabelAverageCrit.Content = AverageCrit;
            LabelAverageHit.Content = AverageHit;

            LabelBiggestCrit.Content = BiggestCrit;
            LabelBiggestHit.Content = BiggestHit;

            LabelAverage.Content = AverageTotal;

            LabelTotalHeal.Content = Heal + "↓";
        }


        public string SkillNameIdent => (string) LabelName.Content;

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