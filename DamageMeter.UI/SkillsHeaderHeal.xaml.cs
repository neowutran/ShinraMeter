using System;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillsHeaderHeal
    {
        public static readonly string SkillName = "Skill name";
        public static readonly string SkillId = "Skill Id";
        public static readonly string CritRateHeal = "CCheal";
        public static readonly string TotalDamage = "Dmg";
        public static readonly string DamagePercentage = "% Dmg";
        public static readonly string AverageCrit = "Avg Crit";
        public static readonly string BiggestCrit = "Big Crit";
        public static readonly string BiggestHit = "Big Blk";
        public static readonly string AverageHit = "Avg Blk";
        public static readonly string AverageTotal = "Avg";
        public static readonly string HitsHeal = "Hheal";
        public static readonly string CritsHeal = "Cheal";
        public static readonly string Heal = "Heal";

        public SkillsHeaderHeal()
        {
            InitializeComponent();
            LabelName.Content = SkillName;
            LabelId.Content = SkillId;
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