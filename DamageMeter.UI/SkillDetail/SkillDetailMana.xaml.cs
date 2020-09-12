using System.Windows;
using System.Windows.Input;
using Lang;

namespace DamageMeter.UI.SkillDetail
{
    /// <summary>
    ///     Logique d'interaction pour SkillContent.xaml
    /// </summary>
    public partial class SkillDetailMana
    {
        public SkillDetailMana(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            InitializeComponent();
            Update(skill, skillAggregate);
        }

        public void Update(Tera.Game.Skill skill, SkillAggregate skillAggregate)
        {
            var chained = skill.IsChained;
            var hit = skill.Detail;

            if (skill.IsHotDot) { hit = LP.Mot; }
            if (hit != null) { LabelName.Content = hit; }
            if (chained == true) { LabelName.Content += " " + LP.Chained; }

            LabelName.ToolTip = skill.Id;
            LabelNumberHitMana.Content = skillAggregate.Hits(skill.Id);
            LabelTotalMana.Content = FormatHelpers.Instance.FormatValue(skillAggregate.Amount(skill.Id));
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) { ((ClickThrouWindow)Window.GetWindow(this))?.Move(sender, e); }
    }
}