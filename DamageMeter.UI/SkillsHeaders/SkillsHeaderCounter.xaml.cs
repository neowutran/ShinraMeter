using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillsHeaders
{
    /// <summary>
    ///     Logique d'interaction pour Skill.xaml
    /// </summary>
    public partial class SkillsHeaderCounter
    {
        public static readonly string SkillName = LP.SkillName;
        public static readonly string Hits = LP.Counter;

        public SkillsHeaderCounter()
        {
            InitializeComponent();
            LabelName.Content = SkillName;
            LabelNumberHit.Content = Hits;
        }


        public string SkillNameIdent => (string) LabelName.Content;

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}