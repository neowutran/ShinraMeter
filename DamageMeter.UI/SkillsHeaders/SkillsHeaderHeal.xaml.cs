using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillsHeaderHeal
    {
        public static readonly string SkillName = LP.SkillName;
        public static readonly string CritRateHeal = LP.CritPercent;
        public static readonly string AverageCrit = LP.AverageCrit;
        public static readonly string BiggestCrit = LP.MaxCrit;
        public static readonly string BiggestHit = LP.MaxWhite;
        public static readonly string AverageHit = LP.AvgWhite;
        public static readonly string AverageTotal = LP.Average;
        public static readonly string HitsHeal = LP.Hits;
        public static readonly string CritsHeal = LP.Crits;
        public static readonly string Heal = LP.Heal;

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
            try { w?.DragMove(); }
            catch { Console.WriteLine(@"Exception move"); }
        }
    }
}