using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour SkillsHeaderDps.xaml
    /// </summary>
    public partial class SkillsHeaderDps
    {
        public static readonly string SkillName = LP.SkillName;
        public static readonly string CritRateDmg = LP.CritPercent;
        public static readonly string TotalDamage = LP.Damage;
        public static readonly string DamagePercentage = LP.DamagePercent;
        public static readonly string AverageCrit = LP.AverageCrit;
        public static readonly string BiggestCrit = LP.MaxCrit;
        public static readonly string AverageHit = LP.AvgWhite;
        public static readonly string AverageTotal = LP.Average;
        public static readonly string HitsDmg = LP.Hits;
        public static readonly string CritsDmg = LP.Crits;

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

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}