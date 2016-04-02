using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour SkillsHeaderDps.xaml
    /// </summary>
    public partial class SkillsHeaderDps : UserControl
    {
        public static readonly string SkillName = "Skill name";
        public static readonly string CritRateDmg = "CCdmg";
        public static readonly string TotalDamage = "Dmg";
        public static readonly string DamagePercentage = "% Dmg";
        public static readonly string AverageCrit = "Avg Crit";
        public static readonly string BiggestCrit = "Big Crit";
        public static readonly string AverageHit = "Avg Blk";
        public static readonly string AverageTotal = "Avg";
        public static readonly string HitsDmg = "HDmg";
        public static readonly string CritsDmg = "Cdmg";

        public SkillsHeaderDps()
        {
            InitializeComponent();
            LabelName.Content = SkillName;
            LabelCritRateDmg.Content = CritRateDmg;

            LabelTotalDamage.Content = TotalDamage + "↓";
            LabelDamagePercentage.Content = DamagePercentage;
            LabelNumberHitDmg.Content = HitsDmg;

            LabelAverageCrit.Content = AverageCrit;
            LabelBiggestCrit.Content = BiggestCrit;
            LabelAverageHit.Content = AverageHit;

            LabelNumberCritDmg.Content = CritsDmg;
            LabelAverageTotal.Content = AverageTotal;
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