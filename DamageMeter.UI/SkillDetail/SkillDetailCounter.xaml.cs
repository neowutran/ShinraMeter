using System;
using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailCounter
    {
        public SkillDetailCounter(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            InitializeComponent();
            Update(skill, skillAggregate);
        }

        public void Update(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            var chained = skill.IsChained;
            var hit = skill.Detail;

            if (hit != null) { LabelName.Content = hit; }
            if (chained == true) { LabelName.Content += " " + LP.Chained; }

            LabelName.ToolTip = skill.Id;
            LabelNumberHit.Content = skillAggregate.Hits(skill.Id);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}