using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillsHeaderMana
    {
        public static readonly string SkillName = LP.SkillName;
        public static readonly string HitsMana = LP.Hits;
        public static readonly string Mana = LP.Mana;

        public SkillsHeaderMana()
        {
            InitializeComponent();
            LabelName.Content = SkillName;
            LabelNumberHitMana.Content = HitsMana;
            LabelTotalMana.Content = Mana + "↓";
        }


        public string SkillNameIdent => (string) LabelName.Content;

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}