using System;
using System.Windows;
using System.Windows.Input;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillsHeaderMana
    {
        public static readonly string SkillName = "Skill name";
        public static readonly string SkillId = "Skill Id";
        public static readonly string CritRateDmg = "CCdmg";
        public static readonly string CritRateHeal = "CCheal";
        public static readonly string TotalDamage = "Dmg";
        public static readonly string DamagePercentage = "% Dmg";
        public static readonly string AverageCrit = "Avg Crit";
        public static readonly string BiggestCrit = "Big Crit";
        public static readonly string AverageHit = "Avg Blk";
        public static readonly string AverageTotal = "Avg";
        public static readonly string HitsDmg = "HDmg";
        public static readonly string HitsHeal = "Hheal";
        public static readonly string HitsMana = "Hmana";
        public static readonly string CritsDmg = "Cdmg";
        public static readonly string CritsHeal = "Cheal";
        public static readonly string Mana = "Mana";
        public static readonly string Heal = "Heal";

        public SkillsHeaderMana()
        {
            InitializeComponent();
            LabelName.Content = SkillName;
            LabelId.Content = SkillId;


            LabelNumberHitMana.Content = HitsMana;


            LabelTotalMana.Content = Mana + "↓";
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